using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class HomeController : Controller
{
    private readonly RaceDayApiService _api;
    public HomeController(RaceDayApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var events = await _api.GetEventsAsync();
        return View(events);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
