using FluentValidation;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Resources;

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
            .NotEmpty().WithMessage(ValidationMessages.ModelNameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.ModelNameMaxLength);

        // Provider validation
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage(ValidationMessages.ProviderRequired)
            .MaximumLength(100).WithMessage(ValidationMessages.ProviderMaxLength);

        // Version validation (optional field)
        RuleFor(x => x.Version)
            .MaximumLength(50).WithMessage(ValidationMessages.VersionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Version));

        // Input price validation
        RuleFor(x => x.InputPricePer1M)
            .GreaterThan(0).WithMessage(ValidationMessages.InputPriceGreaterThanZero)
            .PrecisionScale(10, 6, ignoreTrailingZeros: true).WithMessage(ValidationMessages.InputPriceDecimalPlaces);

        // Output price validation
        RuleFor(x => x.OutputPricePer1M)
            .GreaterThan(0).WithMessage(ValidationMessages.OutputPriceGreaterThanZero)
            .PrecisionScale(10, 6, ignoreTrailingZeros: true).WithMessage(ValidationMessages.OutputPriceDecimalPlaces);

        // Currency validation
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(ValidationMessages.CurrencyRequired)
            .Must(c => ValidCurrencies.Contains(c))
            .WithMessage(ValidationMessages.CurrencyInvalid(string.Join(", ", ValidCurrencies)));

        // Status validation
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(ValidationMessages.StatusRequired)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage(ValidationMessages.StatusInvalid(string.Join(", ", ValidStatuses)));

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
                .WithMessage(ValidationMessages.PricingValidFromBeforeValidTo);
        });

        // Capabilities validation (Story 2.6)
        RuleFor(x => x.Capabilities)
            .NotNull().WithMessage(ValidationMessages.CapabilitiesRequired)
            .SetValidator(new CreateCapabilityValidator());
    }
}
