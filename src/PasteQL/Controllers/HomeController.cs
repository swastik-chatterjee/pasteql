using Microsoft.AspNetCore.Mvc;
using PasteQL.Models;

namespace PasteQL.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new PasteCreateViewModel());
    }

    [HttpGet("/Home/Error")]
    public IActionResult Error()
    {
        return View();
    }
}
