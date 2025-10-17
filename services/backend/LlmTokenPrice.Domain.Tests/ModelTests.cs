using FluentAssertions;
using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Domain.Tests;

public class ModelTests
{
    [Fact]
    public void Model_Creation_SetsDefaultValues()
    {
        // Arrange & Act
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4",
            Provider = "OpenAI",
            InputPricePer1M = 30.0m,
            OutputPricePer1M = 60.0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        model.Status.Should().Be("active");
        model.Currency.Should().Be("USD");
        model.IsActive.Should().BeTrue();
        model.BenchmarkScores.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Model_WithVersion_StoresVersionCorrectly()
    {
        // Arrange & Act
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Claude 3 Opus",
            Provider = "Anthropic",
            Version = "20240229",
            InputPricePer1M = 15.0m,
            OutputPricePer1M = 75.0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        model.Version.Should().Be("20240229");
        model.Name.Should().Be("Claude 3 Opus");
        model.Provider.Should().Be("Anthropic");
    }

    [Fact]
    public void Model_WithPricingValidityPeriod_StoresDateRange()
    {
        // Arrange
        var validFrom = new DateTime(2024, 1, 1);
        var validTo = new DateTime(2024, 12, 31);

        // Act
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Gemini Pro",
            Provider = "Google",
            InputPricePer1M = 0.5m,
            OutputPricePer1M = 1.5m,
            PricingValidFrom = validFrom,
            PricingValidTo = validTo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        model.PricingValidFrom.Should().Be(validFrom);
        model.PricingValidTo.Should().Be(validTo);
    }

    [Fact]
    public void Model_CanBeMarkedAsInactive_ForSoftDelete()
    {
        // Arrange & Act
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Deprecated Model",
            Provider = "Legacy Provider",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 20.0m,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        model.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Model_WithCapability_CanSetNavigationProperty()
    {
        // Arrange & Act
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4 Turbo",
            Provider = "OpenAI",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 30.0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 128000,
                SupportsVision = true,
                SupportsFunctionCalling = true
            }
        };

        // Assert
        model.Capability.Should().NotBeNull();
        model.Capability!.ContextWindow.Should().Be(128000);
        model.Capability.SupportsVision.Should().BeTrue();
    }
}
