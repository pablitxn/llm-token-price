using FluentAssertions;
using Moq;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for AdminModelService.CreateModelAsync method.
/// Tests business logic with mocked repository (no database dependencies).
/// Uses Moq for repository mocking and FluentAssertions for assertions.
/// </summary>
public class AdminModelServiceTests
{
    private readonly Mock<IAdminModelRepository> _mockRepository;
    private readonly AdminModelService _service;

    public AdminModelServiceTests()
    {
        _mockRepository = new Mock<IAdminModelRepository>();
        _service = new AdminModelService(_mockRepository.Object);
    }

    #region CreateModelAsync - Success Scenarios

    /// <summary>
    /// [P0] AC 3: CreateModelAsync should create Model entity with all fields from request.
    /// Verifies entity properties match DTO, timestamps set to UTC, IsActive=true.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithValidRequest_ShouldCreateModelWithCorrectProperties()
    {
        // GIVEN: Valid CreateModelRequest
        var request = new CreateModelRequest
        {
            Name = "GPT-4 Turbo",
            Provider = "OpenAI",
            Version = "gpt-4-1106-preview",
            ReleaseDate = "2023-11-06",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            PricingValidFrom = "2024-01-01",
            PricingValidTo = "2024-12-31"
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns new GUID
        var expectedGuid = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var beforeExecution = DateTime.UtcNow;

        // WHEN: Creating model via service
        var result = await _service.CreateModelAsync(request, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // THEN: Returns expected GUID
        result.Should().Be(expectedGuid);

        // THEN: CreateModelAsync called with correct Model entity
        _mockRepository.Verify(r => r.CreateModelAsync(
            It.Is<Model>(m =>
                m.Name == request.Name &&
                m.Provider == request.Provider &&
                m.Version == request.Version &&
                m.Status == request.Status &&
                m.InputPricePer1M == request.InputPricePer1M &&
                m.OutputPricePer1M == request.OutputPricePer1M &&
                m.Currency == request.Currency
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    /// <summary>
    /// [P0] AC 4: CreateModelAsync should create Capability entity with default values.
    /// Verifies ContextWindow=0, all flags=false except SupportsStreaming=true, ModelId FK set.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithValidRequest_ShouldCreateCapabilityWithDefaults()
    {
        // GIVEN: Valid CreateModelRequest
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns new GUID
        var expectedGuid = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating model via service
        await _service.CreateModelAsync(request, CancellationToken.None);

        // THEN: CreateCapabilityAsync called with correct defaults
        _mockRepository.Verify(r => r.CreateCapabilityAsync(
            It.Is<Capability>(c =>
                c.ContextWindow == 0 &&
                c.MaxOutputTokens == null &&
                c.SupportsFunctionCalling == false &&
                c.SupportsVision == false &&
                c.SupportsAudioInput == false &&
                c.SupportsAudioOutput == false &&
                c.SupportsStreaming == true && // Only this flag is true
                c.SupportsJsonMode == false
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    /// <summary>
    /// [P0] AC 5: CreateModelAsync should return created model GUID.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithValidRequest_ShouldReturnModelGuid()
    {
        // GIVEN: Valid CreateModelRequest
        var request = new CreateModelRequest
        {
            Name = "Claude 3",
            Provider = "Anthropic",
            Status = "active",
            InputPricePer1M = 15.00m,
            OutputPricePer1M = 75.00m,
            Currency = "USD"
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns specific GUID
        var expectedGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating model via service
        var result = await _service.CreateModelAsync(request, CancellationToken.None);

        // THEN: Returns expected GUID
        result.Should().Be(expectedGuid);
    }

    /// <summary>
    /// [P1] AC 3,4: CreateModelAsync should call CreateModelAsync and CreateCapabilityAsync in order.
    /// Verifies both repository methods are called exactly once.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithValidRequest_ShouldCallRepositoryMethodsInOrder()
    {
        // GIVEN: Valid CreateModelRequest
        var request = new CreateModelRequest
        {
            Name = "Gemini Pro",
            Provider = "Google",
            Status = "active",
            InputPricePer1M = 5.00m,
            OutputPricePer1M = 15.00m,
            Currency = "USD"
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns new GUID
        var expectedGuid = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating model via service
        await _service.CreateModelAsync(request, CancellationToken.None);

        // THEN: Duplicate check called once
        _mockRepository.Verify(r => r.GetByNameAndProviderAsync(
            request.Name,
            request.Provider,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        // THEN: CreateModelAsync called once
        _mockRepository.Verify(r => r.CreateModelAsync(
            It.IsAny<Model>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        // THEN: CreateCapabilityAsync called once
        _mockRepository.Verify(r => r.CreateCapabilityAsync(
            It.IsAny<Capability>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion

    #region CreateModelAsync - Duplicate Detection

    /// <summary>
    /// [P1] AC 6: CreateModelAsync should throw InvalidOperationException when duplicate model exists.
    /// Tests duplicate detection by Name + Provider (case-insensitive).
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithDuplicateNameAndProvider_ShouldThrowInvalidOperationException()
    {
        // GIVEN: CreateModelRequest with duplicate Name + Provider
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // Mock repository: duplicate exists
        var existingModel = new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingModel);

        // WHEN/THEN: Creating model should throw InvalidOperationException
        var act = async () => await _service.CreateModelAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A model with name 'GPT-4' and provider 'OpenAI' already exists.");

        // THEN: CreateModelAsync and CreateCapabilityAsync should NOT be called
        _mockRepository.Verify(r => r.CreateModelAsync(
            It.IsAny<Model>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);

        _mockRepository.Verify(r => r.CreateCapabilityAsync(
            It.IsAny<Capability>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    /// <summary>
    /// [P1] AC 6: Duplicate check should be case-insensitive for Name and Provider.
    /// </summary>
    [Theory]
    [InlineData("gpt-4", "openai")] // lowercase
    [InlineData("GPT-4", "OPENAI")] // uppercase
    [InlineData("GpT-4", "OpEnAi")] // mixed case
    public async Task CreateModelAsync_DuplicateCheck_ShouldBeCaseInsensitive(string name, string provider)
    {
        // GIVEN: CreateModelRequest with different casing
        var request = new CreateModelRequest
        {
            Name = name,
            Provider = provider,
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // Mock repository: duplicate exists with different casing
        var existingModel = new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4", // Different casing
            Provider = "OpenAI", // Different casing
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(name, provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingModel);

        // WHEN/THEN: Creating model should throw InvalidOperationException
        var act = async () => await _service.CreateModelAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A model with name '{name}' and provider '{provider}' already exists.");
    }

    #endregion

    #region CreateModelAsync - Date Parsing

    /// <summary>
    /// [P2] AC 3: CreateModelAsync should parse optional date fields correctly.
    /// Verifies ReleaseDate, PricingValidFrom, PricingValidTo converted to DateTime.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithDateFields_ShouldParseDatesCorrectly()
    {
        // GIVEN: CreateModelRequest with date strings
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            ReleaseDate = "2023-03-14",
            PricingValidFrom = "2024-01-01",
            PricingValidTo = "2024-12-31"
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns new GUID
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating model via service
        await _service.CreateModelAsync(request, CancellationToken.None);

        // THEN: Dates parsed correctly
        _mockRepository.Verify(r => r.CreateModelAsync(
            It.Is<Model>(m =>
                m.ReleaseDate == DateTime.Parse("2023-03-14") &&
                m.PricingValidFrom == DateTime.Parse("2024-01-01") &&
                m.PricingValidTo == DateTime.Parse("2024-12-31")
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    /// <summary>
    /// [P2] AC 3: CreateModelAsync should handle null optional date fields.
    /// </summary>
    [Fact]
    public async Task CreateModelAsync_WithNullDateFields_ShouldSetNullInEntity()
    {
        // GIVEN: CreateModelRequest with null date fields
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.00m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            ReleaseDate = null,
            PricingValidFrom = null,
            PricingValidTo = null
        };

        // Mock repository: no duplicate exists
        _mockRepository
            .Setup(r => r.GetByNameAndProviderAsync(request.Name, request.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Mock repository: CreateModelAsync returns new GUID
        _mockRepository
            .Setup(r => r.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Mock repository: CreateCapabilityAsync succeeds
        _mockRepository
            .Setup(r => r.CreateCapabilityAsync(It.IsAny<Capability>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating model via service
        await _service.CreateModelAsync(request, CancellationToken.None);

        // THEN: Null dates passed to repository
        _mockRepository.Verify(r => r.CreateModelAsync(
            It.Is<Model>(m =>
                m.ReleaseDate == null &&
                m.PricingValidFrom == null &&
                m.PricingValidTo == null
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion
}
