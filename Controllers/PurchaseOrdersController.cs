using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Purchasing;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Purchase orders.
/// </summary>
/// <remarks>
/// Replaces <c>frmProformaInvoice.frm</c> and <c>frmProformaInvoiceOC.frm</c> — around 4,100 lines
/// of VB6 across two near-identical forms. The differences between them are recorded in
/// <c>docs/analysis/po-currency-variant-diff.md</c>.
/// </remarks>
[Authorize]
public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderService _orders;
    private readonly ISupplierService _suppliers;
    private readonly IItemService _items;

    /// <summary>Initialises a new instance.</summary>
    public PurchaseOrdersController(
        IPurchaseOrderService orders,
        ISupplierService suppliers,
        IItemService items)
    {
        _orders = orders;
        _suppliers = suppliers;
        _items = items;
    }

    /// <summary>Lists purchase orders.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? supplierId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new PurchaseOrderIndexViewModel
        {
            Results = await _orders.SearchAsync(search, supplierId, page, 25, cancellationToken),
            SearchTerm = search,
            SupplierId = supplierId,
            Suppliers = await LoadSuppliersAsync(cancellationToken),
        });
    }

    /// <summary>Shows one order with its lines.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.PurchaseOrders.View)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _orders.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.PurchaseOrders.Create)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new PurchaseOrderFormViewModel();
        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Creates an order and its lines.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.PurchaseOrders.Create)]
    public async Task<IActionResult> Create(
        PurchaseOrderFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _orders.CreateAsync(ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] =
            $"Transaction Update successfully completed. P.O. No = {result.Value.Number}";

        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _orders.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var order = result.Value;

        var model = new PurchaseOrderFormViewModel
        {
            Id = order.Id,
            Number = order.Number,
            SupplierId = order.SupplierId,
            OrderDate = order.OrderDate,
            FromDate = order.FromDate,
            ToDate = order.ToDate,
            Lines = order.Lines
                .Select(l => new PurchaseOrderLineInputModel
                {
                    ItemId = l.ItemId,
                    Description = l.Description,
                    PrNo = l.PrNo,
                    Quantity = l.Quantity,
                    QtyUnit = l.QtyUnit,
                    UnitPrice = l.UnitPrice,
                    Currency = l.Currency,
                    Condition = l.Condition,
                })
                .ToList(),
        };

        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Updates an order and replaces its lines.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Edit(
        int id,
        PurchaseOrderFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _orders.UpdateAsync(id, ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] =
            $"Transaction Update successfully completed. P.O. No = {result.Value.Number}";

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Deletes an order, unless a goods receipt references it.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.PurchaseOrders.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _orders.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Purchase order was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static SavePurchaseOrderDto ToDto(PurchaseOrderFormViewModel model) => new(
        model.SupplierId,
        model.OrderDate,
        model.FromDate,
        model.ToDate,
        model.Lines
            .Select(l => new SavePurchaseOrderLineDto(
                l.ItemId, l.Description, l.PrNo, l.Quantity, l.QtyUnit, l.UnitPrice, l.Currency, l.Condition))
            .ToList());

    private async Task PopulateAsync(PurchaseOrderFormViewModel model, CancellationToken cancellationToken)
    {
        model.Suppliers = await LoadSuppliersAsync(cancellationToken);

        var items = await _items.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Items = items.Items.Select(i => new NamedItemDto(i.Id, i.Name)).ToList();
    }

    private async Task<IReadOnlyList<NamedItemDto>> LoadSuppliersAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        return suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();
    }
}
