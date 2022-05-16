using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Authorization;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>Items. Replaces <c>frmNewItemName.frm</c>.</summary>
[Authorize]
public class ItemsController : Controller
{
    private readonly IItemService _items;
    private readonly IItemTypeService _itemTypes;

    /// <summary>Initialises a new instance.</summary>
    public ItemsController(IItemService items, IItemTypeService itemTypes)
    {
        _items = items;
        _itemTypes = itemTypes;
    }

    /// <summary>Lists items, filtered by name and item type, paged.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? itemTypeId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new ItemIndexViewModel
        {
            Results = await _items.SearchAsync(search, itemTypeId, page, 25, cancellationToken),
            SearchTerm = search,
            ItemTypeId = itemTypeId,
            ItemTypes = await _itemTypes.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View("Form", new ItemFormViewModel
        {
            ItemTypes = await _itemTypes.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Creates an item.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(ItemFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await RedisplayAsync(model, cancellationToken);
        }

        var result = await _items.CreateAsync(new SaveItemDto(model.Name, model.ItemTypeId), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return await RedisplayAsync(model, cancellationToken);
        }

        TempData["Success"] = $"Item '{result.Value.Name}' was saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _items.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View("Form", new ItemFormViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            ItemTypeId = result.Value.ItemTypeId,
            ItemTypes = await _itemTypes.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Updates an item.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, ItemFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return await RedisplayAsync(model, cancellationToken);
        }

        var result = await _items.UpdateAsync(id, new SaveItemDto(model.Name, model.ItemTypeId), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return await RedisplayAsync(model, cancellationToken);
        }

        TempData["Success"] = $"Item '{result.Value.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes an item, provided nothing still references it.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _items.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Item was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Repopulates the drop-down before redisplaying a form that failed validation.</summary>
    private async Task<IActionResult> RedisplayAsync(ItemFormViewModel model, CancellationToken cancellationToken)
    {
        model.ItemTypes = await _itemTypes.ListAllAsync(cancellationToken);
        return View("Form", model);
    }
}
