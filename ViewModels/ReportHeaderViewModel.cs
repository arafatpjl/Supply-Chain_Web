using SupplyChain.Application.Reports;

namespace SupplyChain.Web.ViewModels;

/// <summary>
/// The heading shown above a rendered report, with its export link.
/// </summary>
/// <remarks>
/// Shared by every report screen so the company name, criteria, row count and PDF button behave
/// identically. The legacy reports each carried their own header inside a hand-built <c>.rpt</c>
/// file, which is why they do not agree on layout.
/// </remarks>
public class ReportHeaderViewModel
{
    /// <summary>Report title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Company name, or a description of the scope.</summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>Company address, where reporting on a single company.</summary>
    public string? CompanyAddress { get; init; }

    /// <summary>The parameters in force.</summary>
    public IReadOnlyList<ReportCriterion> Criteria { get; init; } = Array.Empty<ReportCriterion>();

    /// <summary>Number of rows returned.</summary>
    public int RowCount { get; init; }

    /// <summary>When the report was run.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Who ran it.</summary>
    public string GeneratedBy { get; init; } = string.Empty;

    /// <summary>Path the PDF export links to.</summary>
    public string ExportPath { get; init; } = string.Empty;

    /// <summary>Query values carried to the export link, so it exports what is on screen.</summary>
    public IDictionary<string, string> ExportRouteValues { get; init; } = new Dictionary<string, string>();

    /// <summary>Builds a header from a report result.</summary>
    /// <typeparam name="TRow">One row of output.</typeparam>
    /// <param name="report">The report just run.</param>
    /// <param name="exportPath">Path the export link points at.</param>
    /// <param name="routeValues">Current query values, so the export matches the screen.</param>
    public static ReportHeaderViewModel From<TRow>(
        ReportResult<TRow> report,
        string exportPath,
        IDictionary<string, string> routeValues) => new()
        {
            Title = report.Title,
            CompanyName = report.CompanyName,
            CompanyAddress = report.CompanyAddress,
            Criteria = report.Criteria,
            RowCount = report.Rows.Count,
            GeneratedAt = report.GeneratedAt,
            GeneratedBy = report.GeneratedBy,
            ExportPath = exportPath,
            ExportRouteValues = routeValues,
        };

    /// <summary>Builds the export URL, preserving the filters currently applied.</summary>
    public string BuildExportUrl()
    {
        var parts = ExportRouteValues
            .Where(kv => !string.Equals(kv.Key, "format", StringComparison.OrdinalIgnoreCase))
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}")
            .Append("format=pdf");

        return $"{ExportPath}?{string.Join("&", parts)}";
    }
}
