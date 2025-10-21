using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Application.Validators;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Domain.Services;
using LlmTokenPrice.Infrastructure.Auth;
using LlmTokenPrice.Infrastructure.Caching;
using LlmTokenPrice.Infrastructure.Data;
using LlmTokenPrice.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// FluentValidation configuration
builder.Services.AddValidatorsFromAssemblyContaining<CreateModelValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Localization configuration (Story 2.13 Task 13: Spanish/English validation messages)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "es" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    // Read language from Accept-Language header
    options.ApplyCurrentCultureToResponseHeaders = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LLM Token Price Comparison API",
        Version = "v1",
        Description = "REST API for LLM model pricing and benchmark data comparison"
    });
});

// CORS configuration (must allow credentials for JWT cookies)
// Task 18: Environment-based CORS configuration for production security
var corsOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]
    ?? builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:5173"; // Fallback for development

var allowedOrigins = corsOrigins
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Required for HttpOnly cookies
});

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }
    )
);

// Redis cache configuration (singleton connection multiplexer)
// Note: Redis is optional - app will function without it (graceful degradation)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var connectionString = builder.Configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogWarning("Redis connection string not configured. Application will run without caching.");
        return null!; // Null-forgiving operator: Graceful degradation allows null, consumers must handle
    }

    try
    {
        var configurationOptions = ConfigurationOptions.Parse(connectionString);
        configurationOptions.AbortOnConnectFail = false; // Graceful degradation
        configurationOptions.ConnectTimeout = 5000; // 5 seconds
        configurationOptions.SyncTimeout = 5000;

        var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);

        if (multiplexer.IsConnected)
        {
            logger.LogInformation("Redis connected successfully to {Endpoints}",
                string.Join(", ", multiplexer.GetEndPoints().Select(e => e.ToString())));
        }
        else
        {
            logger.LogWarning("Redis connection established but not fully connected. Cache operations may be degraded.");
        }

        return multiplexer;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to Redis. Application will continue without caching.");
        return null!; // Null-forgiving operator: Return null on connection failure for graceful degradation
    }
});

// Cache repository (scoped)
builder.Services.AddScoped<ICacheRepository, RedisCacheRepository>();

// Domain repositories (scoped)
builder.Services.AddScoped<IModelRepository, ModelRepository>();
builder.Services.AddScoped<IAdminModelRepository, AdminModelRepository>();
builder.Services.AddScoped<IBenchmarkRepository, BenchmarkRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Story 2.13 Task 14: Audit log repository

// Domain services (transient - pure business logic, no state)
builder.Services.AddTransient<BenchmarkNormalizer>();

// Security services (Story 2.13 Task 17: XSS protection)
builder.Services.AddSingleton<LlmTokenPrice.Infrastructure.Security.InputSanitizationService>();

// Application services (scoped)
builder.Services.AddScoped<IModelQueryService, ModelQueryService>();
builder.Services.AddScoped<IAdminModelService, AdminModelService>();
builder.Services.AddScoped<IAdminBenchmarkService, AdminBenchmarkService>();
builder.Services.AddScoped<CSVImportService>(); // Story 2.11: CSV bulk import service
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication configuration
// Task 19: Read JWT secret from environment variable (preferred) or configuration (fallback)
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException(
        "JWT SecretKey is not configured. Set JWT_SECRET_KEY environment variable or Jwt:SecretKey in appsettings.json");

// Validate JWT secret strength in production
if (!builder.Environment.IsDevelopment() && jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters for HS256 algorithm in production. " +
        "Generate a secure key with: openssl rand -base64 48");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };

    // Read token from cookie instead of Authorization header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["admin_token"];
            return Task.CompletedTask;
        }
    };
});

// Rate Limiting configuration (Story 2.13 Task 7: Protect admin endpoints from abuse)
// Limits: 100 requests per minute per IP address for /api/admin/* endpoints
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429; // Too Many Requests
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";

    // General rate limit rules
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*/api/admin/*", // Apply to all admin endpoints
            Period = "1m", // Per minute
            Limit = 100 // 100 requests per minute
        }
    };

    // Customize response when rate limit is exceeded
    options.QuotaExceededResponse = new QuotaExceededResponse
    {
        Content = "{{\"error\": {{\"code\": \"RATE_LIMIT_EXCEEDED\", \"message\": \"API rate limit exceeded. Please try again later.\", \"retryAfter\": \"{0}\"}}}}",
        ContentType = "application/json",
        StatusCode = 429
    };
});

// Inject counter and IP policy stores
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();

// Log CORS configuration for debugging (after app is built)
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CORS configured with allowed origins: {Origins}", string.Join(", ", allowedOrigins));

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Security headers middleware (Story 2.13 Task 17: XSS protection via CSP)
app.Use(async (context, next) =>
{
    // Content-Security-Policy: Prevents inline scripts and unsafe code execution
    // This is a strict policy for a data API - no inline scripts allowed
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " + // Allow inline styles for Swagger UI
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'");

    // X-Content-Type-Options: Prevents MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // X-Frame-Options: Prevents clickjacking attacks
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // X-XSS-Protection: Legacy XSS filter (for older browsers)
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Referrer-Policy: Controls referer information sent with requests
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});

app.UseCors(); // Must be before UseAuthentication

// Request localization middleware (Story 2.13 Task 13: Detect language from Accept-Language header)
// Must be early in the pipeline to set culture for the entire request
app.UseRequestLocalization();

// Rate limiting middleware (Story 2.13 Task 7: Must be before UseAuthentication)
app.UseIpRateLimiting();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var dbLogger = services.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(context, dbLogger);
    }
    catch (Exception ex)
    {
        var dbLogger = services.GetRequiredService<ILogger<Program>>();
        dbLogger.LogError(ex, "An error occurred while initializing the database");
    }
}

app.Run();

// Make Program class accessible for integration/E2E tests (WebApplicationFactory)
namespace LlmTokenPrice.API
{
    public partial class Program { }
}
