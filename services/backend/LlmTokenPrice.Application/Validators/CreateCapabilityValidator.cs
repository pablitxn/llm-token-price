using FluentValidation;
using LlmTokenPrice.Application.DTOs;

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
            .WithMessage($"Context window must be at least {MinContextWindow:N0} tokens")
            .LessThanOrEqualTo(MaxContextWindow)
            .WithMessage($"Context window cannot exceed {MaxContextWindow:N0} tokens");

        // Max output tokens validation (conditional - only when provided)
        When(x => x.MaxOutputTokens.HasValue, () =>
        {
            RuleFor(x => x.MaxOutputTokens!.Value)
                .GreaterThan(0)
                .WithMessage("Max output tokens must be greater than 0")
                .LessThanOrEqualTo(x => x.ContextWindow)
                .WithMessage("Max output tokens cannot exceed context window");
        });
    }
}
