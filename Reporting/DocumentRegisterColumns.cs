using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.Registers;

namespace SupplyChain.Web.Reporting;

/// <summary>Printed columns for the purchase order register.</summary>
public static class PurchaseOrderRegisterColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<PurchaseOrderRegisterRow>> Definition =
    [
        new("P.O. No", r => r.OrderNumber, 1.3f),
        new("P.O. Date", r => r.OrderDate.ToString("dd-MMM-yyyy"), 1.1f),
        new("Supplier", r => r.SupplierName, 2f),
        new("Item", r => r.ItemName, 1.8f),
        new("Type", r => r.ItemTypeName, 1.2f),
        new("PR No.", r => r.PrNo, 1f),
        new("Qty", r => r.Quantity.ToString("N2"), 0.9f, IsNumeric: true, Total: r => r.Quantity),
        new("Unit", r => r.QtyUnit ?? string.Empty, 0.7f),
        new("Unit Price", r => r.UnitPrice.ToString("N2"), 1f, IsNumeric: true),
        new("Total", r => r.LineTotal.ToString("N2"), 1.1f, IsNumeric: true, Total: r => r.LineTotal),
    ];
}

/// <summary>Printed columns for the goods receipt register.</summary>
public static class MrrRegisterColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<MrrRegisterRow>> Definition =
    [
        new("MRR No", r => r.MrrNumber, 1.3f),
        new("MRR Date", r => r.ReceiptDate.ToString("dd-MMM-yyyy"), 1.1f),
        new("Supplier", r => r.SupplierName, 1.8f),
        new("Challan", r => $"{r.ChallanNo} {r.ChallanDate:dd-MMM-yy}", 1.4f),
        new("Item", r => r.ItemName, 1.8f),
        new("PR No.", r => r.PrNo, 1f),
        new("Ordered", r => r.OrderedQty.ToString("N2"), 0.9f, IsNumeric: true, Total: r => r.OrderedQty),
        new("Received", r => r.ReceivedQty.ToString("N2"), 0.9f, IsNumeric: true, Total: r => r.ReceivedQty),
        new("Unit", r => r.QtyUnit ?? string.Empty, 0.7f),
        new("Total", r => r.LineTotal.ToString("N2"), 1.1f, IsNumeric: true, Total: r => r.LineTotal),
    ];
}

/// <summary>Printed columns for the item movement report.</summary>
public static class ItemMovementColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<ItemMovementRow>> Definition =
    [
        new("Company", r => r.CompanyName, 1.8f),
        new("Type", r => r.ItemTypeName, 1.2f),
        new("Item", r => r.ItemName, 2f),
        new("Ordered", r => r.OrderedQty.ToString("N2"), 1f, IsNumeric: true, Total: r => r.OrderedQty),
        new("Received", r => r.ReceivedQty.ToString("N2"), 1f, IsNumeric: true, Total: r => r.ReceivedQty),
        new("Outstanding", r => r.OutstandingQty.ToString("N2"), 1.1f, IsNumeric: true, Total: r => r.OutstandingQty),
        new("Ordered Value", r => r.OrderedValue.ToString("N2"), 1.2f, IsNumeric: true, Total: r => r.OrderedValue),
        new("Received Value", r => r.ReceivedValue.ToString("N2"), 1.2f, IsNumeric: true, Total: r => r.ReceivedValue),
    ];
}
