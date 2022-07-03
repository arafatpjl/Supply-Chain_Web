using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.Registers;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>The bill register screen. Replaces sixteen Crystal files.</summary>
public class BillRegisterViewModel
{
    /// <summary>Which filter is applied.</summary>
    [Display(Name = "Filter By")]
    public BillRegisterMode Mode { get; set; } = BillRegisterMode.IdentificationDate;

    /// <summary>Restrict to cash or credit; blank covers both.</summary>
    [Display(Name = "Type")]
    public BillKind? Kind { get; set; }

    /// <summary>The number, for the reference modes.</summary>
    [Display(Name = "Reference")]
    public string? Reference { get; set; }

    /// <summary>Start of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>The party, for the party mode.</summary>
    [Display(Name = "Party")]
    public int? PartyId { get; set; }

    /// <summary>The head, for the accounts-head mode.</summary>
    [Display(Name = "Head of Accounts")]
    public int? AccountsHeadId { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Parties available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Parties { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Accounts heads available for selection.</summary>
    public IReadOnlyList<NamedItemDto> AccountsHeads { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The report output, once run.</summary>
    public ReportResult<BillRegisterRow>? Report { get; set; }

    /// <summary>Whether the chosen mode filters on a date range.</summary>
    public bool UsesDateRange => Mode is BillRegisterMode.IdentificationDate
        or BillRegisterMode.BillDate or BillRegisterMode.MrrDate;

    /// <summary>Whether the chosen mode filters on a typed reference.</summary>
    public bool UsesReference => Mode is BillRegisterMode.IdentificationNo
        or BillRegisterMode.BillNo or BillRegisterMode.MrrNo;
}

/// <summary>The cheque register screen. Replaces three Crystal files.</summary>
public class ChequeRegisterViewModel
{
    /// <summary>Which filter is applied.</summary>
    [Display(Name = "Filter By")]
    public ChequeRegisterMode Mode { get; set; } = ChequeRegisterMode.IssueDate;

    /// <summary>The book, for the book mode.</summary>
    [Display(Name = "Cheque Book")]
    public int? ChequeBookId { get; set; }

    /// <summary>Start of the issue-date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of that range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Matched against the beneficiary name.</summary>
    [Display(Name = "Beneficiary")]
    public string? Beneficiary { get; set; }

    /// <summary>Restrict to cheques not yet honoured.</summary>
    [Display(Name = "Outstanding only")]
    public bool OutstandingOnly { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Cheque books available for selection.</summary>
    public IReadOnlyList<NamedItemDto> ChequeBooks { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The report output, once run.</summary>
    public ReportResult<ChequeRegisterRow>? Report { get; set; }
}

/// <summary>The audit report screen. Replaces four Crystal files across three forms.</summary>
public class AuditReportViewModel
{
    /// <summary>Which kind of document to report on.</summary>
    [Display(Name = "Document Type")]
    public AuditSubject Subject { get; set; } = AuditSubject.PurchaseOrders;

    /// <summary>Start of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of the range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Restrict to deleted documents.</summary>
    [Display(Name = "Deleted only")]
    public bool DeletedOnly { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>The report output, once run.</summary>
    public ReportResult<AuditRow>? Report { get; set; }
}
