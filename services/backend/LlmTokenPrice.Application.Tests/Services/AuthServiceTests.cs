using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for AuthService
/// Tests JWT generation, credential validation, and authentication logic
/// </summary>
public class AuthServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Configure in-memory configuration for testing
        var configurationData = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-32-characters-minimum-for-hs256",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpirationHours"] = "24",
            ["Admin:Username"] = "testadmin",
            ["Admin:Password"] = "testpassword123"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        _authService = new AuthService(_configuration);
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken_WithCorrectClaims()
    {
        // Arrange
        const string username = "testadmin";
        const string role = "admin";

        // Act
        var token = _authService.GenerateJwtToken(username, role);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Verify claims
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);

        // Verify issuer and audience
        jwtToken.Issuer.Should().Be("test-issuer");
        jwtToken.Audiences.Should().Contain("test-audience");

        // Verify expiration (should be 24 hours from now)
        var expirationTime = jwtToken.ValidTo;
        expirationTime.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ValidateCredentials_ShouldReturnTrue_WhenCredentialsAreCorrect()
    {
        // Arrange
        const string username = "testadmin";
        const string password = "testpassword123";

        // Act
        var isValid = _authService.ValidateCredentials(username, password);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCredentials_ShouldReturnFalse_WhenUsernameIsIncorrect()
    {
        // Arrange
        const string username = "wrongadmin";
        const string password = "testpassword123";

        // Act
        var isValid = _authService.ValidateCredentials(username, password);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateCredentials_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string username = "testadmin";
        const string password = "wrongpassword";

        // Act
        var isValid = _authService.ValidateCredentials(username, password);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var loginDto = new LoginDto("testadmin", "testpassword123");

        // Act
        var result = await _authService.AuthenticateAsync(loginDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Authentication successful");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginDto = new LoginDto("testadmin", "wrongpassword");

        // Act
        var result = await _authService.AuthenticateAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenUsernameIsEmpty()
    {
        // Arrange
        var loginDto = new LoginDto("", "password123");

        // Act
        var result = await _authService.AuthenticateAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Username and password are required");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenPasswordIsEmpty()
    {
        // Arrange
        var loginDto = new LoginDto("testadmin", "");

        // Act
        var result = await _authService.AuthenticateAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Username and password are required");
    }

    [Fact]
    public void GenerateJwtToken_ShouldThrowException_WhenSecretKeyIsNotConfigured()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var authService = new AuthService(emptyConfig);

        // Act & Assert
        var act = () => authService.GenerateJwtToken("testadmin", "admin");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT secret key is not configured");
    }
}
