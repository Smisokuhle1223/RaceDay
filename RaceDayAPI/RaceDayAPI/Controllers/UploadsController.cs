using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Filters;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blob;

        public UploadsController(RaceDayDbContext context, BlobStorageService blob)
        {
            _context = context;
            _blob = blob;
        }

        private static string? ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "No file was uploaded.";
            if (file.Length > 5 * 1024 * 1024) return "Image must be 5MB or smaller.";

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return "Only JPG, PNG, GIF or WEBP images are allowed.";

            return null;
        }

        // POST /api/events/{eventId}/banner - Organiser who owns the event
        [RequireRole("Organiser")]
        [HttpPost("api/events/{eventId}/banner")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBanner(int eventId, [FromForm] ImageUploadDto dto)
        {
            var error = ValidateImage(dto.File);
            if (error != null) return BadRequest(new { message = error });

            if (!_blob.IsConfigured)
                return StatusCode(503, new { message = "Image storage is not configured." });

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            int userId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (ev.OrganiserID != userId)
                return StatusCode(403, new { message = "You can only upload banners for events you created." });

            try
            {
                ev.BannerUrl = await _blob.UploadImageAsync(dto.File, "banners");
                await _context.SaveChangesAsync();
                return Ok(new { ev.BannerUrl });
            }
            catch (Exception)
            {
                return StatusCode(502, new { message = "The image could not be uploaded. Please try again." });
            }
        }

        // POST /api/users/profile/picture - any logged-in user
        [RequireRole("Organiser", "Participant")]
        [HttpPost("api/users/profile/picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] ImageUploadDto dto)
        {
            var error = ValidateImage(dto.File);
            if (error != null) return BadRequest(new { message = error });

            if (!_blob.IsConfigured)
                return StatusCode(503, new { message = "Image storage is not configured." });

            int userId = HttpContext.Session.GetInt32("UserID")!.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            try
            {
                user.ProfilePictureUrl = await _blob.UploadImageAsync(dto.File, "profiles");
                await _context.SaveChangesAsync();
                return Ok(new { user.ProfilePictureUrl });
            }
            catch (Exception)
            {
                return StatusCode(502, new { message = "The image could not be uploaded. Please try again." });
            }
        }
    }
}