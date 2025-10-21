using FluentValidation;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Resources;

namespace LlmTokenPrice.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateBenchmarkRequest DTO.
/// Validates all fields except BenchmarkName (which is immutable).
/// </summary>
/// <remarks>
/// Note: BenchmarkName cannot be updated (immutable identifier).
/// Validation rules match CreateBenchmarkValidator except no unique name check needed.
/// </remarks>
public class UpdateBenchmarkValidator : AbstractValidator<UpdateBenchmarkRequest>
{
    private static readonly string[] ValidCategories = ["Reasoning", "Code", "Math", "Language", "Multimodal"];
    private static readonly string[] ValidInterpretations = ["HigherBetter", "LowerBetter"];

    public UpdateBenchmarkValidator()
    {
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
