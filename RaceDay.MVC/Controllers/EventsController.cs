using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class EventsController : Controller
{
    private readonly RaceDayApiService _api;
    public EventsController(RaceDayApiService api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev == null) return NotFound();

        var categories = await _api.GetCategoriesAsync(id);
        ViewData["Categories"] = categories;
        ViewData["Role"] = HttpContext.Session.GetString("Role");
        return View(ev);
    }
}
