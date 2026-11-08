using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Models;

namespace RaceDayWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
            if (role == "Participant") return RedirectToAction("Index", "Events");
            return View();
        }

        public IActionResult AccessDenied() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}