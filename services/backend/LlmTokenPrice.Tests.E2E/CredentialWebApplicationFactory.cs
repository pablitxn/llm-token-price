using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>Owns ephemeral credentials for one in-process authentication fixture.</summary>
public class CredentialWebApplicationFactory : WebApplicationFactory<LlmTokenPrice.API.Program>
{
    public string AdminUsername { get; } = "unit-" + Guid.NewGuid().ToString("N");
    public string AdminPassword { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public string JwtSigningKey { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(48));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:Username"] = AdminUsername,
                ["Admin:Password"] = AdminPassword,
                ["Jwt:SecretKey"] = JwtSigningKey,
                ["Jwt:Issuer"] = "fixture-issuer",
                ["Jwt:Audience"] = "fixture-audience",
                ["Jwt:ExpirationHours"] = "1"
            }));
        builder.ConfigureServices(services =>
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSigningKey));
                options.TokenValidationParameters.ValidIssuer = "fixture-issuer";
                options.TokenValidationParameters.ValidAudience = "fixture-audience";
            }));
    }
}
