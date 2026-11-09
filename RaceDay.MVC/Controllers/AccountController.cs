using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly RaceDayApiService _api;
    public AccountController(RaceDayApiService api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.LoginAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Login failed.");
            return View(vm);
        }

        HttpContext.Session.SetInt32("UserId", result.Data!.UserId);
        HttpContext.Session.SetString("Role", result.Data.Role);
        HttpContext.Session.SetString("UserName", $"{result.Data.FirstName} {result.Data.LastName}");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.RegisterAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Registration failed.");
            return View(vm);
        }

        // Auto-login after registration
        var login = await _api.LoginAsync(new LoginVm { Email = vm.Email, Password = vm.Password });
        if (login.Success)
        {
            HttpContext.Session.SetInt32("UserId", login.Data!.UserId);
            HttpContext.Session.SetString("Role", login.Data.Role);
            HttpContext.Session.SetString("UserName", $"{login.Data.FirstName} {login.Data.LastName}");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
