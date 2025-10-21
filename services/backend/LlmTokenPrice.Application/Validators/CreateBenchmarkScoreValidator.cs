using FluentValidation;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Resources;

namespace LlmTokenPrice.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateBenchmarkScoreDto.
/// Validates score data before persisting to database.
/// </summary>
/// <remarks>
/// Key validation rules:
/// - BenchmarkId: Required (must be a valid GUID)
/// - Score: Required, must be a finite decimal number
/// - MaxScore: Optional, but must be >= Score when provided
/// - SourceUrl: Optional, but must be valid URL format when provided
/// - Notes: Optional, max 500 characters
///
/// Business Rules:
/// - Out-of-range scores: NOT validated here (handled as warning in service layer)
/// - Duplicate check (ModelId + BenchmarkId): Handled in service layer, not validator
/// - NormalizedScore: Calculated server-side, not validated from user input
/// </remarks>
public class CreateBenchmarkScoreValidator : AbstractValidator<CreateBenchmarkScoreDto>
{
    public CreateBenchmarkScoreValidator()
    {
        // BenchmarkId validation
        RuleFor(x => x.BenchmarkId)
            .NotEmpty().WithMessage(ValidationMessages.BenchmarkRequired);

        // Score validation
        RuleFor(x => x.Score)
            .NotNull().WithMessage(ValidationMessages.ScoreRequired);

        // MaxScore validation (only when provided)
        When(x => x.MaxScore.HasValue, () =>
        {
            // Cross-field validation: Score <= MaxScore
            RuleFor(x => x)
                .Must(x => x.Score <= x.MaxScore!.Value)
                .WithMessage(ValidationMessages.ScoreExceedsMaxScore)
                .WithName(nameof(CreateBenchmarkScoreDto.Score));
        });

        // TestDate validation (no specific rules, nullable DateTime is fine)

        // SourceUrl validation (only when provided)
        When(x => !string.IsNullOrWhiteSpace(x.SourceUrl), () =>
        {
            RuleFor(x => x.SourceUrl)
                .Must(BeValidUrl).WithMessage(ValidationMessages.SourceUrlInvalidFormat)
                .When(x => !string.IsNullOrWhiteSpace(x.SourceUrl));
        });

        // Notes validation (optional, max length)
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(ValidationMessages.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Verified field: No validation needed (boolean with default)
    }

    /// <summary>
    /// Validates that a string is a well-formed absolute URL.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    /// <returns>True if valid absolute URL, false otherwise.</returns>
    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true; // Null/empty is valid for optional fields

        // UriKind.Absolute requires scheme (http://, https://, etc.)
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
