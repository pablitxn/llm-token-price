using FluentAssertions;
using FluentValidation.TestHelper;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Validators;

namespace LlmTokenPrice.Application.Tests.Validators;

/// <summary>
/// Unit tests for CreateModelValidator FluentValidation rules.
/// Tests all validation constraints for Story 2.5 acceptance criteria.
/// Uses FluentValidation.TestHelper for testing validation logic.
/// </summary>
public class CreateModelValidatorTests
{
    private readonly CreateModelValidator _validator;

    public CreateModelValidatorTests()
    {
        _validator = new CreateModelValidator();
    }

    #region Required Fields Validation

    /// <summary>
    /// [P0] AC 2: Name is required - validation should fail when Name is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NameRequired_ShouldFail_WhenNameIsNullOrEmpty(string? name)
    {
        // GIVEN: CreateModelRequest with null/empty Name
        var request = new CreateModelRequest
        {
            Name = name!,
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Name
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Model name is required");
    }

    /// <summary>
    /// [P0] AC 2: Provider is required - validation should fail when Provider is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ProviderRequired_ShouldFail_WhenProviderIsNullOrEmpty(string? provider)
    {
        // GIVEN: CreateModelRequest with null/empty Provider
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = provider!,
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Provider
        result.ShouldHaveValidationErrorFor(x => x.Provider)
            .WithErrorMessage("Provider is required");
    }

    /// <summary>
    /// [P0] AC 2: Status is required - validation should fail when Status is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_StatusRequired_ShouldFail_WhenStatusIsNullOrEmpty(string? status)
    {
        // GIVEN: CreateModelRequest with null/empty Status
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = status!,
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Status
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status is required");
    }

    /// <summary>
    /// [P0] AC 2: Currency is required - validation should fail when Currency is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_CurrencyRequired_ShouldFail_WhenCurrencyIsNullOrEmpty(string? currency)
    {
        // GIVEN: CreateModelRequest with null/empty Currency
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = currency!
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Currency
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    #endregion

    #region Positive Price Validation

    /// <summary>
    /// [P1] AC 2: InputPricePer1M must be greater than 0 - validation should fail for 0 or negative values.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public void Validate_InputPricePositive_ShouldFail_WhenPriceIsZeroOrNegative(decimal price)
    {
        // GIVEN: CreateModelRequest with 0 or negative InputPricePer1M
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = price,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for InputPricePer1M
        result.ShouldHaveValidationErrorFor(x => x.InputPricePer1M)
            .WithErrorMessage("Input price must be greater than 0");
    }

    /// <summary>
    /// [P1] AC 2: OutputPricePer1M must be greater than 0 - validation should fail for 0 or negative values.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30.00)]
    public void Validate_OutputPricePositive_ShouldFail_WhenPriceIsZeroOrNegative(decimal price)
    {
        // GIVEN: CreateModelRequest with 0 or negative OutputPricePer1M
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = price,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for OutputPricePer1M
        result.ShouldHaveValidationErrorFor(x => x.OutputPricePer1M)
            .WithErrorMessage("Output price must be greater than 0");
    }

    /// <summary>
    /// [P1] AC 2: Valid positive prices should pass validation.
    /// </summary>
    [Fact]
    public void Validate_PositivePrices_ShouldPass_WhenPricesAreGreaterThanZero()
    {
        // GIVEN: CreateModelRequest with valid positive prices
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass for prices
        result.ShouldNotHaveValidationErrorFor(x => x.InputPricePer1M);
        result.ShouldNotHaveValidationErrorFor(x => x.OutputPricePer1M);
    }

    #endregion

    #region Decimal Precision Validation

    /// <summary>
    /// [P1] AC 2: Prices can have max 6 decimal places - validation should fail for 7+ decimals.
    /// </summary>
    [Theory]
    [InlineData(10.1234567)] // 7 decimals
    [InlineData(10.12345678)] // 8 decimals
    public void Validate_DecimalPrecision_ShouldFail_WhenPriceHasMoreThan6Decimals(decimal price)
    {
        // GIVEN: CreateModelRequest with price having more than 6 decimal places
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = price,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for InputPricePer1M
        result.ShouldHaveValidationErrorFor(x => x.InputPricePer1M)
            .WithErrorMessage("Input price can have maximum 6 decimal places");
    }

    /// <summary>
    /// [P1] AC 2: Prices with 6 or fewer decimals should pass validation.
    /// </summary>
    [Theory]
    [InlineData(10.123456)] // 6 decimals (max allowed)
    [InlineData(10.12345)] // 5 decimals
    [InlineData(10.5)] // 1 decimal
    [InlineData(10)] // 0 decimals
    public void Validate_DecimalPrecision_ShouldPass_WhenPriceHas6OrFewerDecimals(decimal price)
    {
        // GIVEN: CreateModelRequest with price having 6 or fewer decimal places
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = price,
            OutputPricePer1M = price,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass for prices
        result.ShouldNotHaveValidationErrorFor(x => x.InputPricePer1M);
        result.ShouldNotHaveValidationErrorFor(x => x.OutputPricePer1M);
    }

    #endregion

    #region String Length Validation

    /// <summary>
    /// [P1] AC 2: Name cannot exceed 255 characters - validation should fail when Name is too long.
    /// </summary>
    [Fact]
    public void Validate_NameLength_ShouldFail_WhenNameExceeds255Characters()
    {
        // GIVEN: CreateModelRequest with Name exceeding 255 characters
        var request = new CreateModelRequest
        {
            Name = new string('A', 256), // 256 characters
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Name
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Model name cannot exceed 255 characters");
    }

    /// <summary>
    /// [P1] AC 2: Provider cannot exceed 100 characters - validation should fail when Provider is too long.
    /// </summary>
    [Fact]
    public void Validate_ProviderLength_ShouldFail_WhenProviderExceeds100Characters()
    {
        // GIVEN: CreateModelRequest with Provider exceeding 100 characters
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = new string('B', 101), // 101 characters
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Provider
        result.ShouldHaveValidationErrorFor(x => x.Provider)
            .WithErrorMessage("Provider cannot exceed 100 characters");
    }

    /// <summary>
    /// [P2] AC 2: Version cannot exceed 50 characters - validation should fail when Version is too long.
    /// </summary>
    [Fact]
    public void Validate_VersionLength_ShouldFail_WhenVersionExceeds50Characters()
    {
        // GIVEN: CreateModelRequest with Version exceeding 50 characters
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Version = new string('C', 51), // 51 characters
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Version
        result.ShouldHaveValidationErrorFor(x => x.Version)
            .WithErrorMessage("Version cannot exceed 50 characters");
    }

    #endregion

    #region Enum Validation

    /// <summary>
    /// [P1] AC 2: Status must be valid enum - validation should fail for invalid Status values.
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("ACTIVE")] // Case-sensitive
    [InlineData("beta-test")]
    public void Validate_StatusEnum_ShouldFail_WhenStatusIsInvalid(string status)
    {
        // GIVEN: CreateModelRequest with invalid Status
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = status,
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Status
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status must be one of: active, deprecated, beta");
    }

    /// <summary>
    /// [P1] AC 2: Status must be valid enum - validation should pass for valid Status values.
    /// </summary>
    [Theory]
    [InlineData("active")]
    [InlineData("deprecated")]
    [InlineData("beta")]
    public void Validate_StatusEnum_ShouldPass_WhenStatusIsValid(string status)
    {
        // GIVEN: CreateModelRequest with valid Status
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = status,
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass for Status
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    /// <summary>
    /// [P1] AC 2: Currency must be valid enum - validation should fail for invalid Currency values.
    /// </summary>
    [Theory]
    [InlineData("ARS")]
    [InlineData("JPY")]
    [InlineData("usd")] // Case-sensitive
    public void Validate_CurrencyEnum_ShouldFail_WhenCurrencyIsInvalid(string currency)
    {
        // GIVEN: CreateModelRequest with invalid Currency
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = currency
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for Currency
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be one of: USD, EUR, GBP");
    }

    /// <summary>
    /// [P1] AC 2: Currency must be valid enum - validation should pass for valid Currency values.
    /// </summary>
    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Validate_CurrencyEnum_ShouldPass_WhenCurrencyIsValid(string currency)
    {
        // GIVEN: CreateModelRequest with valid Currency
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = currency
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass for Currency
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }

    #endregion

    #region Date Range Validation

    /// <summary>
    /// [P2] AC 2: PricingValidFrom must be before PricingValidTo when both provided.
    /// </summary>
    [Fact]
    public void Validate_DateRange_ShouldFail_WhenValidFromIsAfterValidTo()
    {
        // GIVEN: CreateModelRequest with ValidFrom after ValidTo
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            PricingValidFrom = "2024-12-31",
            PricingValidTo = "2024-01-01"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should fail for date range
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Pricing Valid From must be before Pricing Valid To");
    }

    /// <summary>
    /// [P2] AC 2: Date range validation should pass when ValidFrom is before ValidTo.
    /// </summary>
    [Fact]
    public void Validate_DateRange_ShouldPass_WhenValidFromIsBeforeValidTo()
    {
        // GIVEN: CreateModelRequest with ValidFrom before ValidTo
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            PricingValidFrom = "2024-01-01",
            PricingValidTo = "2024-12-31"
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass for date range
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    /// <summary>
    /// [P2] AC 2: Date range validation should be skipped when only one date is provided.
    /// </summary>
    [Theory]
    [InlineData("2024-01-01", null)]
    [InlineData(null, "2024-12-31")]
    [InlineData(null, null)]
    public void Validate_DateRange_ShouldPass_WhenOnlyOneDateProvided(string? validFrom, string? validTo)
    {
        // GIVEN: CreateModelRequest with only one date or neither
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            PricingValidFrom = validFrom,
            PricingValidTo = validTo
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass (date range check skipped)
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Optional Fields

    /// <summary>
    /// [P2] AC 2: Optional fields (Version, ReleaseDate, pricing dates) should allow null values.
    /// </summary>
    [Fact]
    public void Validate_OptionalFields_ShouldPass_WhenOptionalFieldsAreNull()
    {
        // GIVEN: CreateModelRequest with all optional fields null
        var request = new CreateModelRequest
        {
            Name = "GPT-4",
            Provider = "OpenAI",
            Status = "active",
            InputPricePer1M = 10.50m,
            OutputPricePer1M = 30.00m,
            Currency = "USD",
            Version = null,
            ReleaseDate = null,
            PricingValidFrom = null,
            PricingValidTo = null
        };

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass (optional fields can be null)
        result.ShouldNotHaveValidationErrorFor(x => x.Version);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseDate);
        result.ShouldNotHaveValidationErrorFor(x => x.PricingValidFrom);
        result.ShouldNotHaveValidationErrorFor(x => x.PricingValidTo);
    }

    #endregion

    #region Complete Valid Request

    /// <summary>
    /// [P0] AC 1-6: Complete valid request should pass all validation rules.
    /// </summary>
    [Fact]
    public void Validate_CompleteRequest_ShouldPass_WhenAllFieldsAreValid()
    {
        // GIVEN: Complete valid CreateModelRequest
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

        // WHEN: Validating the request
        var result = _validator.TestValidate(request);

        // THEN: Validation should pass completely
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
