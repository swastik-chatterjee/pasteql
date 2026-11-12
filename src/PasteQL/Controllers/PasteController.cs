using Microsoft.AspNetCore.Mvc;
using PasteQL.Models;
using PasteQL.Services;

namespace PasteQL.Controllers;

[Route("[controller]")]
public class PasteController : Controller
{
    private readonly PasteService _pasteService;

    public PasteController(PasteService pasteService)
    {
        _pasteService = pasteService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Display(string id, [FromQuery] string? password = null)
    {
        try
        {
            var paste = await _pasteService.GetByIdAsync(id, password);
            if (paste is null)
            {
                return NotFound();
            }

            return View(paste);
        }
        catch (UnauthorizedAccessException)
        {
            return View("Protected", new PasteProtectionViewModel { Id = id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PasteCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Home/Index.cshtml", model);
        }

        var created = await _pasteService.CreateAsync(new PasteCreateRequest(
            model.Title,
            model.Content,
            model.Language,
            model.Password,
            model.BurnAfterRead,
            model.ExpiresInMinutes,
            model.MaxViews));

        return RedirectToAction(nameof(Display), new { id = created.Slug });
    }

    [HttpPost("{id}/unlock")]
    [ValidateAntiForgeryToken]
    public IActionResult Unlock(string id, [FromForm] string password)
    {
        return RedirectToAction(nameof(Display), new { id, password });
    }
}
