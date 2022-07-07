using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.TransactionStatement;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>
/// The transaction statement screen: its filters and, once run, its results.
/// </summary>
/// <remarks>
/// One screen replaces four VB6 forms. The mode is preset by the menu item the user chose, so
/// "Bill Wise Report" still opens on a bill number field, but there is one query behind all four.
/// </remarks>
public class TransactionStatementViewModel
{
    /// <summary>Which filter is applied.</summary>
    public StatementFilterMode Mode { get; set; } = StatementFilterMode.Transaction;

    /// <summary>The bill, goods receipt or challan number, for the reference modes.</summary>
    [Display(Name = "Reference")]
    public string? Reference { get; set; }

    /// <summary>Start of the date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of the date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Restricts to one transaction type; <c>null</c> means all.</summary>
    [Display(Name = "Type")]
    public TransactionType? Type { get; set; }

    /// <summary>Restricts to one statement; <c>null</c> means all.</summary>
    [Display(Name = "Statement No")]
    public int? StatementNo { get; set; }

    /// <summary>Runs across every company rather than the active one.</summary>
    [Display(Name = "All Company")]
    public bool AllCompanies { get; set; }

    /// <summary>Whether the user may run the report across companies.</summary>
    /// <remarks>
    /// The legacy "All Company" checkbox was on every one of these forms with no permission
    /// behind it. Here the checkbox is only rendered for users who hold the permission, and the
    /// controller refuses the request regardless of what is posted.
    /// </remarks>
    public bool CanRunAllCompanies { get; set; }

    /// <summary>Whether the form has been submitted, so empty results mean "no rows" not "not run yet".</summary>
    public bool HasRun { get; set; }

    /// <summary>The report output, once run.</summary>
    public ReportResult<TransactionStatementRow>? Report { get; set; }

    /// <summary>The label for the reference field, which differs per mode.</summary>
    public string ReferenceLabel => Mode switch
    {
        StatementFilterMode.BillNo => "Bill No",
        StatementFilterMode.MrrNo => "MRR No",
        StatementFilterMode.ChallanNo => "Challan No",
        _ => "Reference",
    };

    /// <summary>Whether this mode filters on a document reference rather than a date range.</summary>
    public bool UsesReference => Mode != StatementFilterMode.Transaction;

    /// <summary>The screen title, matching the legacy menu wording.</summary>
    public string Title => Mode switch
    {
        StatementFilterMode.BillNo => "Bill Wise Report",
        StatementFilterMode.MrrNo => "MRR Wise Report",
        StatementFilterMode.ChallanNo => "Challan Wise Report",
        _ => "Transaction Wise Report",
    };

    /// <summary>The VB6 form this screen replaces, shown as provenance.</summary>
    public string LegacyForm => Mode switch
    {
        StatementFilterMode.BillNo => "frmrptBillWiseReport.frm",
        StatementFilterMode.MrrNo => "frmrptMRRWiseReport.frm",
        StatementFilterMode.ChallanNo => "frmrptChalanWiseReport.frm",
        _ => "frmrptTransactionWiseReport.frm",
    };
}
