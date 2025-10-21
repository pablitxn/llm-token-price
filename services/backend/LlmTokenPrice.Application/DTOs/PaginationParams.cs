namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Pagination parameters for list endpoints.
/// Used as query parameters in API controllers.
/// </summary>
/// <remarks>
/// Story 2.13 Task 5: Pagination implementation.
///
/// Usage in controllers:
/// <code>
/// public async Task<ActionResult> GetModels([FromQuery] PaginationParams pagination)
/// </code>
///
/// Default values:
/// - Page: 1 (first page)
/// - PageSize: 20 (reasonable default for web UIs)
/// - MaxPageSize: 100 (prevent abuse and performance issues)
///
/// Page numbering: 1-indexed (page=1 is first page)
/// </remarks>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    /// <summary>
    /// Page number (1-indexed).
    /// Default: 1
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// Default: 20
    /// Max: 100
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Validates pagination parameters.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return Page >= 1 && PageSize >= 1 && PageSize <= MaxPageSize;
    }

    /// <summary>
    /// Calculates the number of items to skip for the current page.
    /// Used with LINQ Skip() method.
    /// </summary>
    /// <returns>Number of items to skip (0-indexed offset).</returns>
    public int GetSkip()
    {
        return (Page - 1) * PageSize;
    }

    /// <summary>
    /// Creates a cache key suffix for pagination parameters.
    /// Used to create unique cache keys per page.
    /// </summary>
    /// <returns>Cache key suffix (e.g., ":p1:s20")</returns>
    public string ToCacheKeySuffix()
    {
        return $":p{Page}:s{PageSize}";
    }
}
