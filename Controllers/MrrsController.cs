using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Purchasing;
using SupplyChain.Application.Receiving;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Goods receipts (MRR) and the purchase order balance they reconcile against.
/// </summary>
/// <remarks>
/// Replaces <c>frmMrrReceiveChallan.frm</c> and <c>frmMrrReceiveChallanOtherCurr.frm</c> —
/// approximately 6,900 lines across two near-identical forms, one of which deleted from the other's
/// table. See <c>docs/analysis/mrr-currency-variant-diff.md</c>.
/// </remarks>
[Authorize]
public class MrrsController : Controller
{
    private readonly IMrrService _receipts;
    private readonly ISupplierService _suppliers;
    private readonly IItemService _items;
    private readonly IPurchaseOrderService _orders;

    /// <summary>Initialises a new instance.</summary>
    public MrrsController(
        IMrrService receipts,
        ISupplierService suppliers,
        IItemService items,
        IPurchaseOrderService orders)
    {
        _receipts = receipts;
        _suppliers = suppliers;
        _items = items;
        _orders = orders;
    }

    /// <summary>Lists goods receipts.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Mrr.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? supplierId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new MrrIndexViewModel
        {
            Results = await _receipts.SearchAsync(search, supplierId, page, 25, cancellationToken),
            SearchTerm = search,
            SupplierId = supplierId,
            Suppliers = await LoadSuppliersAsync(cancellationToken),
        });
    }

    /// <summary>Shows one receipt with its lines and approvals.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Mrr.View)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _receipts.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Mrr.Create)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new MrrFormViewModel();
        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Creates a receipt, its lines and its approvals.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Mrr.Create)]
    public async Task<IActionResult> Create(MrrFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _receipts.CreateAsync(ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] =
            $"Transaction update successfully completed. MRR No = {result.Value.Number}";

        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Mrr.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _receipts.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var receipt = result.Value;

        var model = new MrrFormViewModel
        {
            Id = receipt.Id,
            Number = receipt.Number,
            SupplierId = receipt.SupplierId,
            ReceiptDate = receipt.ReceiptDate,
            Currency = receipt.Currency,
            Charges = receipt.Charges,
            Discount = receipt.Discount,
            QualityCertifiedBy = receipt.Approval?.QualityCertifiedBy,
            PreparedBy = receipt.Approval?.PreparedBy,
            QuantityCheckedBy = receipt.Approval?.QuantityCheckedBy,
            ManagerBy = receipt.Approval?.ManagerBy,
            SeniorManagerBy = receipt.Approval?.SeniorManagerBy,
            Lines = receipt.Lines
                .Select(l => new MrrLineInputModel
                {
                    ItemId = l.ItemId,
                    PurchaseOrderId = l.PurchaseOrderId,
                    Description = l.Description,
                    MaterialCode = l.MaterialCode,
                    PrNo = l.PrNo,
                    OrderedQty = l.OrderedQty,
                    ReceivedQty = l.ReceivedQty,
                    QtyUnit = l.QtyUnit,
                    UnitPrice = l.UnitPrice,
                    ChallanNo = l.ChallanNo,
                    ChallanDate = l.ChallanDate,
                    MainPf = l.MainPf,
                    ContainerNo = l.ContainerNo,
                    Remarks = l.Remarks,
                })
                .ToList(),
        };

        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Updates a receipt and replaces its lines.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Mrr.Edit)]
    public async Task<IActionResult> Edit(int id, MrrFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _receipts.UpdateAsync(id, ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] =
            $"Transaction update successfully completed. MRR No = {result.Value.Number}";

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Deletes a receipt.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Mrr.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _receipts.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Goods receipt was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Ordered against received, per purchase order line.
    /// </summary>
    /// <remarks>
    /// Replaces <c>rptPOBalanceQty.rpt</c>, deferred from Wave 5 because it needs goods receipts to
    /// mean anything.
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> Balance(
        PurchaseOrderBalanceViewModel model,
        CancellationToken cancellationToken)
    {
        var balances = await _receipts.GetBalancesAsync(
            model.PurchaseOrderId, model.SupplierId, cancellationToken);

        model.Balances = model.OverReceivedOnly
            ? balances.Where(b => b.IsOverReceived).ToList()
            : balances;

        model.Suppliers = await LoadSuppliersAsync(cancellationToken);

        var orders = await _orders.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.PurchaseOrders = orders.Items
            .Select(o => new NamedItemDto(o.Id, o.Number))
            .ToList();

        return View(model);
    }

    private static SaveMrrDto ToDto(MrrFormViewModel model) => new(
        model.SupplierId,
        model.ReceiptDate,
        model.Currency,
        model.Charges,
        model.Discount,
        model.Lines
            .Select(l => new SaveMrrLineDto(
                l.ItemId, l.PurchaseOrderId, l.Description, l.MaterialCode, l.PrNo,
                l.OrderedQty, l.ReceivedQty, l.QtyUnit, l.UnitPrice,
                l.ChallanNo, l.ChallanDate, l.MainPf, l.ContainerNo, l.Remarks))
            .ToList(),
        model.HasApproval
            ? new MrrApprovalDto(
                model.QualityCertifiedBy ?? string.Empty,
                model.PreparedBy ?? string.Empty,
                model.QuantityCheckedBy ?? string.Empty,
                model.ManagerBy ?? string.Empty,
                model.SeniorManagerBy ?? string.Empty)
            : null);

    private async Task PopulateAsync(MrrFormViewModel model, CancellationToken cancellationToken)
    {
        model.Suppliers = await LoadSuppliersAsync(cancellationToken);

        var items = await _items.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Items = items.Items.Select(i => new NamedItemDto(i.Id, i.Name)).ToList();

        var orders = await _orders.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.PurchaseOrders = orders.Items.Select(o => new NamedItemDto(o.Id, o.Number)).ToList();
    }

    private async Task<IReadOnlyList<NamedItemDto>> LoadSuppliersAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        return suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();
    }
}
