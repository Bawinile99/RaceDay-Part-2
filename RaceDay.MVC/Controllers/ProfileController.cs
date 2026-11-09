using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class ProfileController : Controller
{
    private readonly RaceDayApiService _api;
    public ProfileController(RaceDayApiService api) => _api = api;

    private bool IsLoggedIn => HttpContext.Session.GetInt32("UserId") != null;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var profile = await _api.GetProfileAsync();
        if (profile == null) return RedirectToAction("Login", "Account");

        var vm = new UpdateProfileVm
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth
        };
        ViewData["ProfilePictureUrl"] = profile.ProfilePictureUrl;
        ViewData["Email"] = profile.Email;
        ViewData["Role"] = profile.Role;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateProfileVm vm)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.UpdateProfileAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Could not save profile.");
            return View(vm);
        }

        TempData["Success"] = "Profile updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please choose a file.";
            return RedirectToAction("Index");
        }

        using var stream = file.OpenReadStream();
        var result = await _api.UploadProfilePictureAsync(stream, file.FileName, file.ContentType);
        if (result.Success) TempData["Success"] = "Picture updated.";
        else TempData["Error"] = result.Error ?? "Upload failed.";

        return RedirectToAction("Index");
    }
}