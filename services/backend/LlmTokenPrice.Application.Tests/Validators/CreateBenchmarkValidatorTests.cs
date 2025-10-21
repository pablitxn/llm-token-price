using FluentAssertions;
using FluentValidation.TestHelper;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Validators;
using Xunit;

namespace LlmTokenPrice.Application.Tests.Validators;

/// <summary>
/// Unit tests for CreateBenchmarkValidator
/// Story 2.9 - Task 10.3: Test form validation logic
/// Priority: P2 (Medium - validation testing)
/// </summary>
public class CreateBenchmarkValidatorTests
{
    private readonly CreateBenchmarkValidator _validator;

    public CreateBenchmarkValidatorTests()
    {
        _validator = new CreateBenchmarkValidator();
    }

    /// <summary>
    /// [P2] Valid benchmark request should pass validation
    /// </summary>
    [Fact]
    public async Task Validate_ValidBenchmarkRequest_ShouldPassValidation()
    {
        // GIVEN: Valid benchmark request with unique name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Description = "Tests reasoning ability across 57 subjects",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should pass
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// [P2] BenchmarkName is required
    /// </summary>
    [Fact]
    public async Task Validate_MissingBenchmarkName_ShouldFailValidation()
    {
        // GIVEN: Request with empty benchmark name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = string.Empty,
            FullName = "Test Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail with specific error
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.BenchmarkName)
            .WithErrorMessage("Benchmark name is required");
    }

    /// <summary>
    /// [P2] BenchmarkName must be 50 characters or less
    /// </summary>
    [Fact]
    public async Task Validate_BenchmarkNameTooLong_ShouldFailValidation()
    {
        // GIVEN: Request with benchmark name exceeding 50 characters
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = new string('A', 51), // 51 characters
            FullName = "Test Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.BenchmarkName)
            .WithErrorMessage("Benchmark name cannot exceed 50 characters");
    }

    /// <summary>
    /// [P2] BenchmarkName must contain only alphanumeric and underscore characters
    /// </summary>
    [Theory]
    [InlineData("MMLU-Pro")] // Contains dash
    [InlineData("MMLU Pro")] // Contains space
    [InlineData("MMLU@2024")] // Contains @ symbol
    [InlineData("MMLU#1")] // Contains # symbol
    public async Task Validate_BenchmarkNameInvalidCharacters_ShouldFailValidation(string invalidName)
    {
        // GIVEN: Request with invalid benchmark name characters
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = invalidName,
            FullName = "Test Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.BenchmarkName)
            .WithErrorMessage("Benchmark name can only contain letters, numbers, and underscores");
    }

    /// <summary>
    /// [P2] BenchmarkName with valid alphanumeric and underscore characters should pass
    /// </summary>
    [Theory]
    [InlineData("MMLU")]
    [InlineData("HumanEval")]
    [InlineData("GSM8K")]
    [InlineData("Big_Bench_Hard")]
    [InlineData("Model_123")]
    public async Task Validate_BenchmarkNameValidCharacters_ShouldPassValidation(string validName)
    {
        // GIVEN: Request with valid benchmark name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = validName,
            FullName = "Test Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should pass
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// [P2] Duplicate benchmark name validation moved to service layer
    /// Story 2.9 AC#6: Validation ensures benchmark names are unique
    /// Note: Unique name check is now performed in AdminBenchmarkService to support synchronous validation
    /// </summary>
    [Fact(Skip = "Unique name validation moved to AdminBenchmarkService layer to support synchronous validation pipeline")]
    public async Task Validate_DuplicateBenchmarkName_ShouldFailValidation()
    {
        // This test is no longer applicable as unique name validation
        // is performed in the service layer (AdminBenchmarkService.CreateBenchmarkAsync)
        // The service throws InvalidOperationException which is caught by the controller
        await Task.CompletedTask;
    }

    /// <summary>
    /// [P2] FullName is required
    /// </summary>
    [Fact]
    public async Task Validate_MissingFullName_ShouldFailValidation()
    {
        // GIVEN: Request with empty full name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = string.Empty,
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    /// <summary>
    /// [P2] TypicalRangeMin must be less than TypicalRangeMax
    /// Story 2.9 - Task 2.7/2.8: Range validation
    /// </summary>
    [Fact]
    public async Task Validate_TypicalRangeMinGreaterThanMax_ShouldFailValidation()
    {
        // GIVEN: Request with min > max
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 100,
            TypicalRangeMax = 0,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TypicalRangeMax)
            .WithErrorMessage("Typical range minimum must be less than maximum");
    }

    /// <summary>
    /// [P2] WeightInQaps must be between 0 and 1
    /// Story 2.9 - Task 2.9: QAPS weight validation
    /// </summary>
    [Theory]
    [InlineData(-0.01)] // Below 0
    [InlineData(1.01)]  // Above 1
    public async Task Validate_WeightInQapsOutOfRange_ShouldFailValidation(decimal invalidWeight)
    {
        // GIVEN: Request with invalid weight
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = invalidWeight
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.WeightInQaps);
    }

    /// <summary>
    /// [P2] WeightInQaps can have at most 2 decimal places
    /// </summary>
    [Theory]
    [InlineData(0.123)] // 3 decimal places
    [InlineData(0.9999)] // 4 decimal places
    public async Task Validate_WeightInQapsTooManyDecimals_ShouldFailValidation(decimal invalidWeight)
    {
        // GIVEN: Request with too many decimal places
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = invalidWeight
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.WeightInQaps)
            .WithErrorMessage("QAPS weight can have maximum 2 decimal places");
    }

    /// <summary>
    /// [P2] Valid WeightInQaps values should pass validation
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.30)]
    [InlineData(0.25)]
    [InlineData(1.00)]
    [InlineData(0.99)]
    public async Task Validate_WeightInQapsValid_ShouldPassValidation(decimal validWeight)
    {
        // GIVEN: Request with valid weight
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = validWeight
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should pass
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// [P2] Invalid Category value should fail validation
    /// </summary>
    [Fact]
    public async Task Validate_InvalidCategory_ShouldFailValidation()
    {
        // GIVEN: Request with invalid category
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "InvalidCategory",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    /// <summary>
    /// [P2] Invalid Interpretation value should fail validation
    /// </summary>
    [Fact]
    public async Task Validate_InvalidInterpretation_ShouldFailValidation()
    {
        // GIVEN: Request with invalid interpretation
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "InvalidInterpretation",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Validating the request
        var result = await _validator.TestValidateAsync(request);

        // THEN: Validation should fail
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Interpretation);
    }
}
