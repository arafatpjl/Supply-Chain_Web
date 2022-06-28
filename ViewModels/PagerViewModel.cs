namespace SupplyChain.Web.ViewModels;

/// <summary>
/// Backs the shared paging control.
/// </summary>
/// <remarks>
/// Carries the query-string values so that paging links preserve the active filters. Losing the
/// filter on page two is a small bug that is easy to introduce and irritating to use.
/// </remarks>
public class PagerViewModel
{
    /// <summary>The one-based current page.</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Total pages available.</summary>
    public int TotalPages { get; set; }

    /// <summary>Total records matching the query.</summary>
    public int TotalCount { get; set; }

    /// <summary>Base path that page links are built from, e.g. <c>/Suppliers/Index</c>.</summary>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>Query-string values to carry across pages, excluding the page number.</summary>
    public IDictionary<string, string?> RouteValues { get; set; } = new Dictionary<string, string?>();

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Whether a further page exists.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>First page number shown in the control, windowed around the current page.</summary>
    public int FirstShownPage => Math.Max(1, PageNumber - 2);

    /// <summary>Last page number shown in the control.</summary>
    public int LastShownPage => Math.Min(TotalPages, FirstShownPage + 4);

    /// <summary>Builds the URL for a given page, preserving the active filters.</summary>
    public string BuildUrl(int page)
    {
        var clamped = Math.Clamp(page, 1, Math.Max(1, TotalPages));

        var parts = RouteValues
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
            .Append($"page={clamped}");

        return $"{BasePath}?{string.Join("&", parts)}";
    }
}
