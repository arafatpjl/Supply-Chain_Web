using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.Registers;

namespace SupplyChain.Web.Reporting;

/// <summary>Printed columns for the bill register.</summary>
public static class BillRegisterColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<BillRegisterRow>> Definition =
    [
        new("ID No", r => r.IdentificationNo, 1.2f),
        new("ID Date", r => r.IdentificationDate.ToString("dd-MMM-yyyy"), 1f),
        new("Type", r => r.Kind.ToString(), 0.7f),
        new("Party", r => r.PartyName, 2f),
        new("Head", r => r.AccountsHeadName, 1.6f),
        new("Bill No", r => r.BillNo, 1.1f),
        new("Bill Date", r => r.BillDate.ToString("dd-MMM-yyyy"), 1f),
        new("MRR No", r => r.MrrNo ?? string.Empty, 1.1f),
        new("Remarks", r => r.Remarks ?? string.Empty, 1.4f),
        new("Amount", r => r.Amount.ToString("N2"), 1.1f, IsNumeric: true, Total: r => r.Amount),
    ];
}

/// <summary>Printed columns for the cheque register.</summary>
public static class ChequeRegisterColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<ChequeRegisterRow>> Definition =
    [
        new("Bank", r => r.BankName, 1.6f),
        new("Book", r => r.BookNo, 0.9f),
        new("Cheque No", r => r.ChequeNo, 1f),
        new("Issue Date", r => r.IssueDate.ToString("dd-MMM-yyyy"), 1.1f),
        new("Beneficiary", r => r.Beneficiary, 2f),
        new("Particulars", r => r.PaymentParticulars ?? string.Empty, 1.6f),
        new("Honour Date", r => r.EncashmentDate?.ToString("dd-MMM-yyyy") ?? "Outstanding", 1.2f),
        new("Value", r => r.Value.ToString("N2"), 1.1f, IsNumeric: true, Total: r => r.Value),
    ];
}

/// <summary>Printed columns for the audit report.</summary>
public static class AuditColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<AuditRow>> Definition =
    [
        new("Document", r => r.DocumentNumber, 1.4f),
        new("Date", r => r.DocumentDate.ToString("dd-MMM-yyyy"), 1f),
        new("Created By", r => r.CreatedBy, 1.3f),
        new("Created", r => r.CreatedAt.ToString("dd-MMM-yy HH:mm"), 1.2f),
        new("Modified By", r => r.ModifiedBy ?? string.Empty, 1.3f),
        new("Modified", r => r.ModifiedAt?.ToString("dd-MMM-yy HH:mm") ?? string.Empty, 1.2f),
        new("Deleted By", r => r.DeletedBy ?? string.Empty, 1.3f),
        new("Deleted", r => r.DeletedAt?.ToString("dd-MMM-yy HH:mm") ?? string.Empty, 1.2f),
        new("Status", r => r.IsDeleted ? "Deleted" : "Active", 0.9f),
    ];
}
