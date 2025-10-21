using FluentValidation;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Resources;

namespace LlmTokenPrice.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateBenchmarkRequest DTO.
/// Validates all required fields, enum values, and range validation.
/// </summary>
/// <remarks>
/// Key validation rules:
/// - BenchmarkName: Required, max 50 chars, alphanumeric + underscore
/// - Category: Must be valid enum value
/// - Interpretation: Must be valid enum value
/// - TypicalRangeMin must be less than TypicalRangeMax
/// - WeightInQaps: 0.00 to 1.00, max 2 decimal places
/// Unique name validation is performed in AdminBenchmarkService layer.
/// </remarks>
public class CreateBenchmarkValidator : AbstractValidator<CreateBenchmarkRequest>
{
    private static readonly string[] ValidCategories = ["Reasoning", "Code", "Math", "Language", "Multimodal"];
    private static readonly string[] ValidInterpretations = ["HigherBetter", "LowerBetter"];

    /// <summary>
    /// Initializes a new instance of the CreateBenchmarkValidator.
    /// </summary>
    public CreateBenchmarkValidator()
    {

        // BenchmarkName validation
        // Note: Unique name check moved to service layer (AdminBenchmarkService)
        // to allow synchronous validation pipeline
        RuleFor(x => x.BenchmarkName)
            .NotEmpty().WithMessage(ValidationMessages.BenchmarkNameRequired)
            .MaximumLength(50).WithMessage(ValidationMessages.BenchmarkNameMaxLength)
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage(ValidationMessages.BenchmarkNameFormat);

        // FullName validation
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(ValidationMessages.FullNameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.FullNameMaxLength);

        // Description validation (optional field)
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage(ValidationMessages.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Category validation
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage(ValidationMessages.CategoryRequired)
            .Must(c => ValidCategories.Contains(c))
            .WithMessage(ValidationMessages.CategoryInvalid(string.Join(", ", ValidCategories)));

        // Interpretation validation
        RuleFor(x => x.Interpretation)
            .NotEmpty().WithMessage(ValidationMessages.InterpretationRequired)
            .Must(i => ValidInterpretations.Contains(i))
            .WithMessage(ValidationMessages.InterpretationInvalid(string.Join(", ", ValidInterpretations)));

        // TypicalRangeMin validation
        RuleFor(x => x.TypicalRangeMin)
            .NotNull().WithMessage(ValidationMessages.TypicalRangeMinRequired);

        // TypicalRangeMax validation
        RuleFor(x => x.TypicalRangeMax)
            .NotNull().WithMessage(ValidationMessages.TypicalRangeMaxRequired);

        // Range validation: Min < Max
        RuleFor(x => x)
            .Must(x => x.TypicalRangeMin < x.TypicalRangeMax)
            .WithMessage(ValidationMessages.TypicalRangeMinLessThanMax)
            .WithName("TypicalRangeMax"); // Error associated with max field

        // WeightInQaps validation
        RuleFor(x => x.WeightInQaps)
            .NotNull().WithMessage(ValidationMessages.QapsWeightRequired)
            .InclusiveBetween(0m, 1m).WithMessage(ValidationMessages.QapsWeightRange)
            .PrecisionScale(3, 2, ignoreTrailingZeros: true).WithMessage(ValidationMessages.QapsWeightDecimalPlaces);
    }
}
