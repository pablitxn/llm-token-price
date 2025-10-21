namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Generic paginated result wrapper for list endpoints.
/// Contains the data items plus pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <remarks>
/// Story 2.13 Task 5: Pagination implementation.
///
/// Response structure:
/// <code>
/// {
///   "items": [...],
///   "pagination": {
///     "currentPage": 1,
///     "pageSize": 20,
///     "totalItems": 150,
///     "totalPages": 8,
///     "hasNextPage": true,
///     "hasPreviousPage": false
///   }
/// }
/// </code>
///
/// Frontend can use pagination metadata to render pagination controls (page numbers, next/prev buttons, etc.)
/// </remarks>
public class PagedResult<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Pagination metadata for rendering pagination controls.
    /// </summary>
    public PaginationMetadata Pagination { get; set; } = new();

    /// <summary>
    /// Creates a paged result from a full list and pagination parameters.
    /// Performs in-memory pagination (use only for already-filtered lists).
    /// </summary>
    /// <param name="allItems">The full list of items to paginate.</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <param name="totalCount">Total number of items (before pagination). If null, uses allItems.Count.</param>
    /// <returns>A paged result with the requested page of items.</returns>
    public static PagedResult<T> Create(
        List<T> allItems,
        PaginationParams paginationParams,
        int? totalCount = null)
    {
        var total = totalCount ?? allItems.Count;
        var items = allItems
            .Skip(paginationParams.GetSkip())
            .Take(paginationParams.PageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            Pagination = PaginationMetadata.Create(
                paginationParams.Page,
                paginationParams.PageSize,
                total)
        };
    }

    /// <summary>
    /// Creates a paged result from items and count (for database-level pagination).
    /// Use when repository already applied Skip/Take.
    /// </summary>
    /// <param name="items">The items for the current page (already paginated).</param>
    /// <param name="page">Current page number.</param>
    /// <param name="pageSize">Page size used.</param>
    /// <param name="totalCount">Total number of items across all pages.</param>
    /// <returns>A paged result with pagination metadata.</returns>
    public static PagedResult<T> Create(
        List<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Pagination = PaginationMetadata.Create(page, pageSize, totalCount)
        };
    }
}

/// <summary>
/// Pagination metadata for a paged result.
/// Provides information for rendering pagination controls in the UI.
/// </summary>
public class PaginationMetadata
{
    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages.
    /// Calculated as: ceiling(totalItems / pageSize)
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a next page available.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Creates pagination metadata from page parameters and total count.
    /// </summary>
    /// <param name="currentPage">Current page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="totalItems">Total number of items across all pages.</param>
    /// <returns>Pagination metadata with calculated values.</returns>
    public static PaginationMetadata Create(int currentPage, int pageSize, int totalItems)
    {
        var totalPages = totalItems > 0
            ? (int)Math.Ceiling(totalItems / (double)pageSize)
            : 0;

        return new PaginationMetadata
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = currentPage < totalPages,
            HasPreviousPage = currentPage > 1
        };
    }
}
