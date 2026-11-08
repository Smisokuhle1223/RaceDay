using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Filters;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers
{
    [RequireLogin]
    public class EventsController : Controller
    {
        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? eventType, string? search, bool upcomingOnly = false)
        {
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(eventType)) qs.Add("eventType=" + Uri.EscapeDataString(eventType));
            if (!string.IsNullOrWhiteSpace(search)) qs.Add("search=" + Uri.EscapeDataString(search));
            if (upcomingOnly) qs.Add("upcomingOnly=true");
            var path = "api/events" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

            var model = new EventsIndexViewModel { EventType = eventType, Search = search, UpcomingOnly = upcomingOnly };

            var res = await _api.GetAsync(path);
            if (res.Ok)
                model.Events = res.Data<List<EventListItem>>() ?? new();
            else
                TempData["Error"] = res.Error;

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var res = await _api.GetAsync($"api/events/{id}");
            if (!res.Ok)
            {
                TempData["Error"] = res.Error;
                return RedirectToAction("Index");
            }

            var ev = res.Data<EventDetail>();
            if (ev == null) return RedirectToAction("Index");
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin("Participant")]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {
            var res = await _api.PostAsync($"api/categories/{categoryId}/enrolments");
            if (res.Ok)
                TempData["Success"] = "You're in! Your enrolment has been confirmed.";
            else
                TempData["Error"] = res.Error;

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}