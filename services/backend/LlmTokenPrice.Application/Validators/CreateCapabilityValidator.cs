using FluentValidation;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Resources;

namespace LlmTokenPrice.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateCapabilityRequest DTO.
/// Validates context window range and max output tokens constraints.
/// </summary>
public class CreateCapabilityValidator : AbstractValidator<CreateCapabilityRequest>
{
    private const int MinContextWindow = 1000;
    private const int MaxContextWindow = 2_000_000;

    public CreateCapabilityValidator()
    {
        // Context window validation
        RuleFor(x => x.ContextWindow)
            .GreaterThanOrEqualTo(MinContextWindow)
            .WithMessage(ValidationMessages.ContextWindowMinimum(MinContextWindow))
            .LessThanOrEqualTo(MaxContextWindow)
            .WithMessage(ValidationMessages.ContextWindowMaximum(MaxContextWindow));

        // Max output tokens validation (conditional - only when provided)
        When(x => x.MaxOutputTokens.HasValue, () =>
        {
            RuleFor(x => x.MaxOutputTokens!.Value)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.MaxOutputTokensGreaterThanZero)
                .LessThanOrEqualTo(x => x.ContextWindow)
                .WithMessage(ValidationMessages.MaxOutputTokensExceedsContextWindow);
        });
    }
}
