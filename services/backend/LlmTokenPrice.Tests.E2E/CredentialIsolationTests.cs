using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using LlmTokenPrice.API.Controllers.Admin;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>Offline configuration and real authentication controls; never starts the API or connects to services.</summary>
public class CredentialIsolationTests
{
    private sealed class ProbeFactory : CredentialWebApplicationFactory
    {
        public void Configure(IWebHostBuilder builder) => ConfigureWebHost(builder);
    }

    private static IWebHost Host(ProbeFactory factory)
    {
        var builder = new WebHostBuilder()
            .UseTestServer()
            .ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Admin:Username"] = "ambient-unit-user",
                    ["Admin:Password"] = "ambient-unit-password",
                    ["Jwt:SecretKey"] = "ambient-unit-signing-material"
                }))
            .ConfigureServices(services => services.AddAuthentication().AddJwtBearer())
            .Configure(_ => { });
        factory.Configure(builder);
        var host = builder.Build();
        host.Start(); // Starts only the synthetic in-process TestServer, never the API entry point.
        return host;
    }

    [Fact]
    public void FactoriesOwnIndependentCredentialsAndOverrideAmbientConfiguration()
    {
        using var first = new ProbeFactory();
        using var second = new ProbeFactory();
        Assert.True(first.AdminUsername != second.AdminUsername, "Fixture identities must be independent");
        Assert.True(first.AdminPassword != second.AdminPassword, "Fixture credentials must be independent");
        Assert.True(first.JwtSigningKey != second.JwtSigningKey, "Fixture keys must be independent");
        Assert.True(first.AdminPassword.Length >= 64);
        Assert.True(first.JwtSigningKey.Length >= 96);

        using var host = Host(first);
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        Assert.True(configuration["Admin:Username"] == first.AdminUsername);
        Assert.True(configuration["Admin:Password"] == first.AdminPassword);
        Assert.True(configuration["Jwt:SecretKey"] == first.JwtSigningKey);
        var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        var key = Assert.IsType<SymmetricSecurityKey>(options.TokenValidationParameters.IssuerSigningKey);
        Assert.True(key.Key.SequenceEqual(Encoding.UTF8.GetBytes(first.JwtSigningKey)));
        Assert.True(options.TokenValidationParameters.ValidIssuer == "fixture-issuer");
        Assert.True(options.TokenValidationParameters.ValidAudience == "fixture-audience");
    }

    [Fact]
    public async Task RealLoginCookieAndSignatureUseOnlyTheOwningFactory()
    {
        using var first = new ProbeFactory();
        using var second = new ProbeFactory();
        using var host = Host(first);
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var service = new AuthService(configuration);
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        var controller = new AdminAuthController(service, NullLogger<AdminAuthController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var response = await controller.Login(new LoginDto(first.AdminUsername, first.AdminPassword));
        Assert.IsType<OkObjectResult>(response.Result);
        var cookie = context.Response.Headers.SetCookie.ToString();
        Assert.True(cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        Assert.True(cookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));
        var token = cookie.Split(';')[0].Split('=', 2)[1];
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = "fixture-issuer",
            ValidateAudience = true, ValidAudience = "fixture-audience",
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(first.JwtSigningKey))
        };
        new JwtSecurityTokenHandler().ValidateToken(token, validation, out var validated);
        Assert.IsType<JwtSecurityToken>(validated);

        validation.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(second.JwtSigningKey));
        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, validation, out _));
        Assert.True(!service.ValidateCredentials(second.AdminUsername, second.AdminPassword));
        Assert.True(!service.ValidateCredentials("ambient-unit-user", "ambient-unit-password"));

        var invalid = await controller.Login(new LoginDto(first.AdminUsername, second.AdminPassword));
        Assert.IsType<UnauthorizedObjectResult>(invalid.Result);
        var empty = await controller.Login(new LoginDto(first.AdminUsername, ""));
        Assert.IsType<BadRequestObjectResult>(empty.Result);
    }
}
