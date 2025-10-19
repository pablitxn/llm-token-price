using FluentValidation;
using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateModelRequest DTO.
/// Validates all required fields, pricing constraints, enum values, and date ranges.
/// </summary>
public class CreateModelValidator : AbstractValidator<CreateModelRequest>
{
    private static readonly string[] ValidCurrencies = ["USD", "EUR", "GBP"];
    private static readonly string[] ValidStatuses = ["active", "deprecated", "beta"];

    public CreateModelValidator()
    {
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Model name is required")
            .MaximumLength(255).WithMessage("Model name cannot exceed 255 characters");

        // Provider validation
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .MaximumLength(100).WithMessage("Provider cannot exceed 100 characters");

        // Version validation (optional field)
        RuleFor(x => x.Version)
            .MaximumLength(50).WithMessage("Version cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Version));

        // Input price validation
        RuleFor(x => x.InputPricePer1M)
            .GreaterThan(0).WithMessage("Input price must be greater than 0")
            .PrecisionScale(10, 6, ignoreTrailingZeros: true).WithMessage("Input price can have maximum 6 decimal places");

        // Output price validation
        RuleFor(x => x.OutputPricePer1M)
            .GreaterThan(0).WithMessage("Output price must be greater than 0")
            .PrecisionScale(10, 6, ignoreTrailingZeros: true).WithMessage("Output price can have maximum 6 decimal places");

        // Currency validation
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(c => ValidCurrencies.Contains(c))
            .WithMessage($"Currency must be one of: {string.Join(", ", ValidCurrencies)}");

        // Status validation
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");

        // Date range validation (conditional - only when both dates provided)
        When(x => !string.IsNullOrEmpty(x.PricingValidFrom) && !string.IsNullOrEmpty(x.PricingValidTo), () =>
        {
            RuleFor(x => x)
                .Must(x => {
                    if (DateTime.TryParse(x.PricingValidFrom, out var from) &&
                        DateTime.TryParse(x.PricingValidTo, out var to))
                    {
                        return from < to;
                    }
                    return true; // Let other validators handle invalid date formats
                })
                .WithMessage("Pricing Valid From must be before Pricing Valid To");
        });

        // Capabilities validation (Story 2.6)
        RuleFor(x => x.Capabilities)
            .NotNull().WithMessage("Capabilities are required")
            .SetValidator(new CreateCapabilityValidator());
    }
}
