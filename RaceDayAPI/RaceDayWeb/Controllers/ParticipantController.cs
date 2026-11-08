using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Filters;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers
{
    [RequireLogin("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> MyEvents()
        {
            var res = await _api.GetAsync("api/enrolments/my");
            if (!res.Ok)
            {
                TempData["Error"] = res.Error;
                return View(new List<MyEnrolment>());
            }
            return View(res.Data<List<MyEnrolment>>() ?? new());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var res = await _api.DeleteAsync($"api/enrolments/{id}");
            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Your enrolment has been cancelled." : res.Error;
            return RedirectToAction("MyEvents");
        }

        public async Task<IActionResult> MyResults()
        {
            int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var res = await _api.GetAsync($"api/users/{userId}/results");
            if (!res.Ok)
            {
                TempData["Error"] = res.Error;
                return View(new List<MyResult>());
            }
            var results = res.Data<List<MyResult>>() ?? new();
            return View(results.OrderByDescending(r => r.EventDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var res = await _api.GetAsync("api/users/profile");
            var profile = res.Ok ? res.Data<ProfileData>() : null;
            if (profile == null)
            {
                TempData["Error"] = res.Ok ? "We couldn't load your profile." : res.Error;
                return RedirectToAction("Index", "Events");
            }

            return View(new ProfileFormModel
            {
                FullName = profile.FullName,
                Email = profile.Email,
                ProfilePictureUrl = profile.ProfilePictureUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var res = await _api.PutAsync("api/users/profile", new { fullName = model.FullName, email = model.Email });
            if (!res.Ok)
            {
                ModelState.AddModelError("", res.Error);
                return View(model);
            }

            HttpContext.Session.SetString("FullName", model.FullName);
            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                TempData["Error"] = "Please choose an image first.";
                return RedirectToAction("Profile");
            }

            var res = await _api.UploadAsync("api/users/profile/picture", picture);
            TempData[res.Ok ? "Success" : "Error"] = res.Ok ? "Profile picture updated." : res.Error;
            return RedirectToAction("Profile");
        }
    }
}