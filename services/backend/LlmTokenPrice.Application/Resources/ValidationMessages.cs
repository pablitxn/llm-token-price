using System.Globalization;
using System.Resources;

namespace LlmTokenPrice.Application.Resources;

/// <summary>
/// Provides strongly-typed access to localized validation messages.
/// Supports English (default) and Spanish (es) cultures.
/// </summary>
/// <remarks>
/// Uses ResourceManager to load messages from ValidationMessages.resx and ValidationMessages.es.resx.
/// FluentValidation will automatically use the current thread's CultureInfo to select the appropriate language.
/// </remarks>
public static class ValidationMessages
{
    private static readonly ResourceManager ResourceManager =
        new("LlmTokenPrice.Application.Resources.ValidationMessages",
            typeof(ValidationMessages).Assembly);

    // Model validation messages
    public static string ModelNameRequired => GetString(nameof(ModelNameRequired));
    public static string ModelNameMaxLength => GetString(nameof(ModelNameMaxLength));
    public static string ProviderRequired => GetString(nameof(ProviderRequired));
    public static string ProviderMaxLength => GetString(nameof(ProviderMaxLength));
    public static string VersionMaxLength => GetString(nameof(VersionMaxLength));
    public static string InputPriceGreaterThanZero => GetString(nameof(InputPriceGreaterThanZero));
    public static string InputPriceDecimalPlaces => GetString(nameof(InputPriceDecimalPlaces));
    public static string OutputPriceGreaterThanZero => GetString(nameof(OutputPriceGreaterThanZero));
    public static string OutputPriceDecimalPlaces => GetString(nameof(OutputPriceDecimalPlaces));
    public static string CurrencyRequired => GetString(nameof(CurrencyRequired));
    public static string StatusRequired => GetString(nameof(StatusRequired));
    public static string PricingValidFromBeforeValidTo => GetString(nameof(PricingValidFromBeforeValidTo));
    public static string CapabilitiesRequired => GetString(nameof(CapabilitiesRequired));

    // Benchmark validation messages
    public static string BenchmarkNameRequired => GetString(nameof(BenchmarkNameRequired));
    public static string BenchmarkNameMaxLength => GetString(nameof(BenchmarkNameMaxLength));
    public static string BenchmarkNameFormat => GetString(nameof(BenchmarkNameFormat));
    public static string FullNameRequired => GetString(nameof(FullNameRequired));
    public static string FullNameMaxLength => GetString(nameof(FullNameMaxLength));
    public static string DescriptionMaxLength => GetString(nameof(DescriptionMaxLength));
    public static string CategoryRequired => GetString(nameof(CategoryRequired));
    public static string InterpretationRequired => GetString(nameof(InterpretationRequired));
    public static string TypicalRangeMinRequired => GetString(nameof(TypicalRangeMinRequired));
    public static string TypicalRangeMaxRequired => GetString(nameof(TypicalRangeMaxRequired));
    public static string TypicalRangeMinLessThanMax => GetString(nameof(TypicalRangeMinLessThanMax));
    public static string QapsWeightRequired => GetString(nameof(QapsWeightRequired));
    public static string QapsWeightRange => GetString(nameof(QapsWeightRange));
    public static string QapsWeightDecimalPlaces => GetString(nameof(QapsWeightDecimalPlaces));

    // Capability validation messages
    public static string MaxOutputTokensGreaterThanZero => GetString(nameof(MaxOutputTokensGreaterThanZero));
    public static string MaxOutputTokensExceedsContextWindow => GetString(nameof(MaxOutputTokensExceedsContextWindow));

    // Benchmark Score validation messages
    public static string BenchmarkRequired => GetString(nameof(BenchmarkRequired));
    public static string ScoreRequired => GetString(nameof(ScoreRequired));
    public static string ScoreExceedsMaxScore => GetString(nameof(ScoreExceedsMaxScore));
    public static string SourceUrlInvalidFormat => GetString(nameof(SourceUrlInvalidFormat));
    public static string NotesMaxLength => GetString(nameof(NotesMaxLength));

    // Methods with format parameters
    public static string CurrencyInvalid(string validCurrencies) =>
        GetFormattedString(nameof(CurrencyInvalid), validCurrencies);

    public static string StatusInvalid(string validStatuses) =>
        GetFormattedString(nameof(StatusInvalid), validStatuses);

    public static string CategoryInvalid(string validCategories) =>
        GetFormattedString(nameof(CategoryInvalid), validCategories);

    public static string InterpretationInvalid(string validInterpretations) =>
        GetFormattedString(nameof(InterpretationInvalid), validInterpretations);

    public static string ContextWindowMinimum(int minTokens) =>
        GetFormattedString(nameof(ContextWindowMinimum), minTokens);

    public static string ContextWindowMaximum(int maxTokens) =>
        GetFormattedString(nameof(ContextWindowMaximum), maxTokens);

    /// <summary>
    /// Retrieves a localized string by key using the current thread's culture.
    /// </summary>
    private static string GetString(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
               ?? $"[Missing: {key}]";
    }

    /// <summary>
    /// Retrieves a localized string and formats it with the provided arguments.
    /// </summary>
    private static string GetFormattedString(string key, params object[] args)
    {
        var format = GetString(key);
        return string.Format(CultureInfo.CurrentUICulture, format, args);
    }
}
