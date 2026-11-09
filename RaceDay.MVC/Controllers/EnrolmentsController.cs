using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class EnrolmentsController : Controller
{
    private readonly RaceDayApiService _api;
    public EnrolmentsController(RaceDayApiService api) => _api = api;

    private bool IsLoggedIn => HttpContext.Session.GetInt32("UserId") != null;
    private string? Role => HttpContext.Session.GetString("Role");

    // Organiser view — all enrolments for their events
    [HttpGet]
    public async Task<IActionResult> Index(int? eventId)
    {
        if (Role != "Organiser") return RedirectToAction("Login", "Account");

        var enrolments = await _api.GetEnrolmentsAsync();

        if (eventId.HasValue)
            enrolments = enrolments.Where(e => e.EventId == eventId.Value).ToList();

        ViewData["EventId"] = eventId;
        return View(enrolments);
    }

    // Participant view — own enrolments
    [HttpGet]
    public async Task<IActionResult> MyEnrolments()
    {
        if (Role != "Participant") return RedirectToAction("Login", "Account");
        var enrolments = await _api.GetEnrolmentsAsync();
        return View(enrolments);
    }

    // Participant — enrol in an event (POST from Events/Details)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (Role != "Participant") return RedirectToAction("Login", "Account");

        var vm = new CreateEnrolmentVm { EventId = eventId, CategoryId = categoryId };
        var result = await _api.CreateEnrolmentAsync(vm);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "Could not enrol.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        TempData["Success"] = "Enrolled successfully.";
        return RedirectToAction(nameof(MyEnrolments));
    }

    // Participant — cancel an enrolment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        await _api.CancelEnrolmentAsync(id);
        TempData["Success"] = "Enrolment cancelled.";
        return RedirectToAction(nameof(MyEnrolments));
    }
}