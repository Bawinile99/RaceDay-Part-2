using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class ResultsController : Controller
{
    private readonly RaceDayApiService _api;
    public ResultsController(RaceDayApiService api) => _api = api;

    private string? Role => HttpContext.Session.GetString("Role");

    // Organiser — view all results for their events + capture form
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (Role != "Organiser") return RedirectToAction("Login", "Account");

        var results = await _api.GetResultsAsync();
        var enrolments = await _api.GetEnrolmentsAsync();

        // Only enrolments that don't already have a result
        var capturedIds = results.Select(r => r.EnrolmentId).ToHashSet();
        var eligible = enrolments.Where(e => !capturedIds.Contains(e.EnrolmentId)).ToList();

        ViewData["EligibleEnrolments"] = eligible;
        return View(results);
    }

    // Organiser — capture a result
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int enrolmentId, int hours, int minutes, int seconds, int finishingPosition)
    {
        if (Role != "Organiser") return RedirectToAction("Login", "Account");

        var totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
        if (totalSeconds <= 0)
        {
            TempData["Error"] = "Finish time must be greater than zero.";
            return RedirectToAction(nameof(Index));
        }
        if (finishingPosition <= 0)
        {
            TempData["Error"] = "Finishing position must be greater than zero.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new CreateResultVm
        {
            EnrolmentId = enrolmentId,
            FinishTimeSeconds = totalSeconds,
            FinishingPosition = finishingPosition
        };

        var result = await _api.CreateResultAsync(vm);
        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "Could not save result.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Result captured.";
        return RedirectToAction(nameof(Index));
    }

    // Participant — own results
    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        if (Role != "Participant") return RedirectToAction("Login", "Account");
        var results = await _api.GetResultsAsync();
        return View(results);
    }
}
