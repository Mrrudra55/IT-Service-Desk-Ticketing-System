namespace ITServiceDesk.DTOs.Common;

public class PagedResultDto<T>
{
    // The list of items on the current page
    public List<T> Items { get; set; } = new List<T>();

    // Total number of items matching the query across all pages
    public int TotalCount { get; set; }

    // Current page number (1-indexed)
    public int Page { get; set; }

    // Number of items per page
    public int PageSize { get; set; }

    // Total calculated pages
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Helper properties for frontend pagination buttons
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

