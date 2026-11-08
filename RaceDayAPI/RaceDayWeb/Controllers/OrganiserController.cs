using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Filters;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers
{
    [RequireLogin("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        private async Task<DashboardData> LoadDashboard()
        {
            var res = await _api.GetAsync("api/organiser/dashboard");
            if (!res.Ok)
            {
                TempData["Error"] = res.Error;
                return new DashboardData();
            }
            return res.Data<DashboardData>() ?? new DashboardData();
        }

        private async Task<List<VenueItem>> LoadVenues()
        {
            var res = await _api.GetAsync("api/venues");
            return res.Ok ? (res.Data<List<VenueItem>>() ?? new()) : new();
        }

        public async Task<IActionResult> Dashboard() => View(await LoadDashboard());

        public async Task<IActionResult> Events() => View(await LoadDashboard());

        // ---------- Create / edit / delete events ----------

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            ViewBag.Venues = await LoadVenues();
            return View("EventForm", new EventFormModel { EventDate = DateTime.Today.AddMonths(1).AddHours(6) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormModel model)
        {
            ViewBag.Venues = await LoadVenues();
            if (!ModelState.IsValid) return View("EventForm", model);

            var res = await _api.PostAsync("api/events", new
            {
                eventName = model.EventName,
                description = model.Description,
                eventDate = model.EventDate,
                venueID = model.VenueID,
                eventType = model.EventType
            });

            if (!res.Ok)
            {
                ModelState.AddModelError("", res.Error);
                return View("EventForm", model);
            }

            var created = res.Data<EventListItem>();
            if (created == null) return RedirectToAction("Events");

            string message = "Event created. Now add categories so people can enter.";
            if (model.Banner != null && model.Banner.Length > 0)
            {
                var up = await _api.UploadAsync($"api/events/{created.EventID}/banner", model.Banner);
                if (!up.Ok) message = "Event created, but the banner could not be uploaded: " + up.Error;
            }

            TempData["Success"] = message;
            return RedirectToAction("Categories", new { eventId = created.EventID });
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var res = await _api.GetAsync($"api/events/{id}");
            var ev = res.Ok ? res.Data<EventDetail>() : null;
            if (ev == null)
            {
                TempData["Error"] = res.Ok ? "We couldn't find that event." : res.Error;
                return RedirectToAction("Events");
            }

            var venues = await LoadVenues();
            ViewBag.Venues = venues;

            var venueId = venues.FirstOrDefault(v => v.VenueName == ev.VenueName && v.City == ev.City)?.VenueID ?? 0;

            return View("EventForm", new EventFormModel
            {
                EventID = ev.EventID,
                EventName = ev.EventName,
                Description = ev.Description,
                EventDate = ev.EventDate,
                VenueID = venueId,
                EventType = ev.EventType,
                ExistingBannerUrl = ev.BannerUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventFormModel model)
        {
            ViewBag.Venues = await LoadVenues();
            if (!ModelState.IsValid || model.EventID == null) return View("EventForm", model);

            var res = await _api.PutAsync($"api/events/{model.EventID}", new
            {
                eventName = model.EventName,
                description = model.Description,
                eventDate = model.EventDate,
                venueID = model.VenueID,
                eventType = model.EventType
            });

            if (!res.Ok)
            {
                ModelState.AddModelError("", res.Error);
                return View("EventForm", model);
            }

            string message = "Event updated.";
            if (model.Banner != null && model.Banner.Length > 0)
            {
                var up = await _api.UploadAsync($"api/events/{model.EventID}/banner", model.Banner);
                if (!up.Ok) message = "Event updated, but the banner could not be uploaded: " + up.Error;
            }

            TempData["Success"] = message;
            return RedirectToAction("Events");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var res = await _api.DeleteAsync($"api/events/{id}");
            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Event deleted." : res.Error;
            return RedirectToAction("Events");
        }

        // ---------- Categories ----------

        public async Task<IActionResult> Categories(int eventId)
        {
            var res = await _api.GetAsync($"api/events/{eventId}");
            var ev = res.Ok ? res.Data<EventDetail>() : null;
            if (ev == null)
            {
                TempData["Error"] = res.Ok ? "We couldn't find that event." : res.Error;
                return RedirectToAction("Events");
            }
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the category details and try again.";
                return RedirectToAction("Categories", new { eventId = model.EventID });
            }

            var res = await _api.PostAsync($"api/events/{model.EventID}/categories", new
            {
                categoryName = model.CategoryName,
                distanceKM = model.DistanceKM,
                maxParticipants = model.MaxParticipants,
                entryFee = model.EntryFee
            });

            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Category added." : res.Error;
            return RedirectToAction("Categories", new { eventId = model.EventID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var res = await _api.DeleteAsync($"api/categories/{id}");
            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Category removed." : res.Error;
            return RedirectToAction("Categories", new { eventId });
        }

        // ---------- Enrolments and results ----------

        private async Task<List<EnrolmentRow>?> LoadEnrolments(int eventId)
        {
            var evRes = await _api.GetAsync($"api/events/{eventId}");
            var ev = evRes.Ok ? evRes.Data<EventDetail>() : null;
            var res = await _api.GetAsync($"api/events/{eventId}/enrolments");

            if (ev == null || !res.Ok)
            {
                TempData["Error"] = !res.Ok ? res.Error : evRes.Error;
                return null;
            }

            ViewBag.EventID = eventId;
            ViewBag.EventName = ev.EventName;
            return res.Data<List<EnrolmentRow>>() ?? new();
        }

        public async Task<IActionResult> Enrolments(int eventId)
        {
            var rows = await LoadEnrolments(eventId);
            if (rows == null) return RedirectToAction("Events");
            return View(rows);
        }

        public async Task<IActionResult> Results(int eventId)
        {
            var rows = await LoadEnrolments(eventId);
            if (rows == null) return RedirectToAction("Events");
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(ResultFormModel model)
        {
            string? time = null;
            int? position = null;

            if (model.Status == "Finished")
            {
                if (!TimeSpan.TryParse(model.FinishTime, out var ts))
                {
                    TempData["Error"] = "Enter the finish time as hh:mm:ss, for example 01:45:30.";
                    return RedirectToAction("Results", new { eventId = model.EventID });
                }
                if (model.Position == null || model.Position < 1)
                {
                    TempData["Error"] = "Enter the finishing position (1 or higher).";
                    return RedirectToAction("Results", new { eventId = model.EventID });
                }
                time = ts.ToString("c");
                position = model.Position;
            }

            var body = new { finishTime = time, position, status = model.Status };

            ApiResponse res = model.ResultID.HasValue
                ? await _api.PutAsync($"api/results/{model.ResultID}", body)
                : await _api.PostAsync($"api/enrolments/{model.EnrolmentID}/results", body);

            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Result published." : res.Error;
            return RedirectToAction("Results", new { eventId = model.EventID });
        }
    }
}