using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.MonthlyStatement;
using SupplyChain.Application.Reports.PartyStatement;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>
/// The party statement screen.
/// </summary>
/// <remarks>
/// Replaces <c>frmrptPartyStatement.frm</c> and its three Crystal files. The summary switch
/// corresponds to the legacy form's "Summary" checkbox, which chose a different report file.
/// </remarks>
public class PartyStatementViewModel
{
    /// <summary>Restrict to one supplier; blank covers every party.</summary>
    [Display(Name = "Party")]
    public int? SupplierId { get; set; }

    /// <summary>Which date column the range applies to. Legacy label: "Query Type".</summary>
    [Display(Name = "Query Type")]
    public StatementDateBasis DateBasis { get; set; } = StatementDateBasis.TransactionDate;

    /// <summary>Start of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Restrict to one transaction type; <c>null</c> means "All Type".</summary>
    [Display(Name = "Statement Type")]
    public TransactionType? Type { get; set; }

    /// <summary>Show totals per party instead of individual lines.</summary>
    [Display(Name = "Summary")]
    public bool Summary { get; set; }

    /// <summary>Report across every company.</summary>
    [Display(Name = "All Company")]
    public bool AllCompanies { get; set; }

    /// <summary>Whether the user may report across companies.</summary>
    public bool CanRunAllCompanies { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Parties available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Detail output, when not summarising.</summary>
    public ReportResult<PartyStatementRow>? Detail { get; set; }

    /// <summary>Summary output, when summarising.</summary>
    public ReportResult<PartyStatementSummaryRow>? SummaryReport { get; set; }
}

/// <summary>The monthly statement screen. Replaces <c>frmrptMonthlyStatement.frm</c>.</summary>
public class MonthlyStatementViewModel
{
    /// <summary>The year to report.</summary>
    [Display(Name = "Year")]
    [Range(1990, 2100)]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    /// <summary>Restrict to one transaction type; <c>null</c> means "All Type".</summary>
    [Display(Name = "Statement Type")]
    public TransactionType? Type { get; set; }

    /// <summary>Report across every company.</summary>
    [Display(Name = "All Company")]
    public bool AllCompanies { get; set; }

    /// <summary>Whether the user may report across companies.</summary>
    public bool CanRunAllCompanies { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>The report output, once run.</summary>
    public ReportResult<MonthlyStatementRow>? Report { get; set; }
}
