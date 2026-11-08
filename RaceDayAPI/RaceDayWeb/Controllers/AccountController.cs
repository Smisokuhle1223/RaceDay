using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var res = await _api.PostAsync("api/auth/login", new { email = model.Email, password = model.Password });
            if (!res.Ok)
            {
                ModelState.AddModelError("", res.Error);
                return View(model);
            }

            var user = res.Data<LoginResult>();
            if (user == null)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var res = await _api.PostAsync("api/auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!res.Ok)
            {
                ModelState.AddModelError("", res.Error);
                return View(model);
            }

            TempData["Success"] = "Account created! You can log in now.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.PostAsync("api/auth/logout");
            _api.ClearCookie();
            HttpContext.Session.Clear();
            TempData["Success"] = "You've been logged out.";
            return RedirectToAction("Login");
        }
    }
}