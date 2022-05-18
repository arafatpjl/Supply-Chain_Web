using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Authorization;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Item types. Replaces <c>frmNewItemType.frm</c>, labelled "Item Group" on the legacy menu.
/// </summary>
[Authorize]
public class ItemTypesController : Controller
{
    private readonly IItemTypeService _service;

    /// <summary>Initialises a new instance.</summary>
    public ItemTypesController(IItemTypeService service) => _service = service;

    /// <summary>Lists item types, filtered and paged.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.View)]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        var results = await _service.SearchAsync(search, page, pageSize: 25, cancellationToken);
        return View(new ItemTypeIndexViewModel { Results = results, SearchTerm = search });
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public IActionResult Create() => View("Form", new ItemTypeFormViewModel());

    /// <summary>Creates an item type.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(ItemTypeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var result = await _service.CreateAsync(
            new SaveItemTypeDto(model.Name, model.Group, model.ShortName), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View("Form", model);
        }

        TempData["Success"] = $"Item type '{result.Value.Name}' was saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View("Form", new ItemTypeFormViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Group = result.Value.Group,
            ShortName = result.Value.ShortName,
        });
    }

    /// <summary>Updates an item type.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, ItemTypeFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var result = await _service.UpdateAsync(
            id, new SaveItemTypeDto(model.Name, model.Group, model.ShortName), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View("Form", model);
        }

        TempData["Success"] = $"Item type '{result.Value.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes an item type, provided no item is classified under it.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Item type was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
