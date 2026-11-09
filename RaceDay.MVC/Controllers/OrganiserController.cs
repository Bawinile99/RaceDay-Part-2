
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiService _api;
    public OrganiserController(RaceDayApiService api) => _api = api;

    private bool IsOrganiser => HttpContext.Session.GetString("Role") == "Organiser";

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");

        var myEvents = await _api.GetMyEventsAsync();

        ViewBag.TotalEvents = myEvents.Count;
        ViewBag.TotalEnrolments = myEvents.Sum(e => e.EnrolmentCount);
        ViewBag.UpcomingCount = myEvents.Count(e => e.EventDate >= DateTime.Today);

        return View(myEvents);
    }

    [HttpGet]
    public async Task<IActionResult> MyEvents()
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        var events = await _api.GetMyEventsAsync();
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        return View(new CreateEventVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(CreateEventVm vm, IFormFile? banner)
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(vm);

        var created = await _api.CreateEventAsync(vm);
        if (!created.Success)
        {
            ModelState.AddModelError("", created.Error ?? "Could not create event.");
            return View(vm);
        }

        if (banner != null && banner.Length > 0)
        {
            var latest = (await _api.GetMyEventsAsync())
                         .OrderByDescending(e => e.EventId)
                         .FirstOrDefault();

            if (latest != null)
            {
                using var stream = banner.OpenReadStream();
                var uploadResult = await _api.UploadBannerAsync(latest.EventId, stream, banner.FileName, banner.ContentType);
                if (!uploadResult.Success)
                {
                    TempData["Warning"] = "Event created, but banner upload failed: " + uploadResult.Error;
                    return RedirectToAction(nameof(MyEvents));
                }
            }
        }

        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(MyEvents));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        var ev = await _api.GetEventAsync(id);
        if (ev == null) return NotFound();

        var vm = new CreateEventVm
        {
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            Distance = ev.Distance,
            EventType = ev.EventType,
            BannerImageUrl = ev.BannerImageUrl
        };
        ViewData["EventId"] = ev.EventId;
        ViewData["ExistingBanner"] = ev.BannerImageUrl;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, CreateEventVm vm, IFormFile? banner)
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            ViewData["EventId"] = id;
            return View(vm);
        }

        var updated = await _api.UpdateEventAsync(id, vm);
        if (!updated.Success)
        {
            ModelState.AddModelError("", updated.Error ?? "Could not update event.");
            ViewData["EventId"] = id;
            return View(vm);
        }

        if (banner != null && banner.Length > 0)
        {
            using var stream = banner.OpenReadStream();
            await _api.UploadBannerAsync(id, stream, banner.FileName, banner.ContentType);
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(MyEvents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!IsOrganiser) return RedirectToAction("Login", "Account");
        await _api.DeleteEventAsync(id);
        TempData["Success"] = "Event deleted.";
        return RedirectToAction(nameof(MyEvents));
    }
}