using FluentValidation;
using LlmTokenPrice.Application.DTOs;

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
            .NotEmpty().WithMessage("Benchmark name is required")
            .MaximumLength(50).WithMessage("Benchmark name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Benchmark name can only contain letters, numbers, and underscores");

        // FullName validation
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters");

        // Description validation (optional field)
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Category validation
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(c => ValidCategories.Contains(c))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}");

        // Interpretation validation
        RuleFor(x => x.Interpretation)
            .NotEmpty().WithMessage("Interpretation is required")
            .Must(i => ValidInterpretations.Contains(i))
            .WithMessage($"Interpretation must be one of: {string.Join(", ", ValidInterpretations)}");

        // TypicalRangeMin validation
        RuleFor(x => x.TypicalRangeMin)
            .NotNull().WithMessage("Typical range minimum is required");

        // TypicalRangeMax validation
        RuleFor(x => x.TypicalRangeMax)
            .NotNull().WithMessage("Typical range maximum is required");

        // Range validation: Min < Max
        RuleFor(x => x)
            .Must(x => x.TypicalRangeMin < x.TypicalRangeMax)
            .WithMessage("Typical range minimum must be less than maximum")
            .WithName("TypicalRangeMax"); // Error associated with max field

        // WeightInQaps validation
        RuleFor(x => x.WeightInQaps)
            .NotNull().WithMessage("QAPS weight is required")
            .InclusiveBetween(0m, 1m).WithMessage("QAPS weight must be between 0.00 and 1.00")
            .PrecisionScale(3, 2, ignoreTrailingZeros: true).WithMessage("QAPS weight can have maximum 2 decimal places");
    }
}
