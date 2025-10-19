using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Application.Validators;
using LlmTokenPrice.Domain.Repositories;
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
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

// Application services (scoped)
builder.Services.AddScoped<IModelQueryService, ModelQueryService>();
builder.Services.AddScoped<IAdminModelService, AdminModelService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication configuration
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings");

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(); // Must be before UseAuthentication
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
        var logger = services.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(context, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

app.Run();

// Make Program class accessible for integration/E2E tests (WebApplicationFactory)
namespace LlmTokenPrice.API
{
    public partial class Program { }
}
