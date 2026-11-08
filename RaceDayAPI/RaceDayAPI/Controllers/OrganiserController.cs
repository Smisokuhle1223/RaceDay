using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Filters;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class OrganiserController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganiserController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/organiser/dashboard - Organiser only
        [RequireRole("Organiser")]
        [HttpGet("api/organiser/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            int organiserId = HttpContext.Session.GetInt32("UserID")!.Value;

            var events = await _context.Events
                .Where(e => e.OrganiserID == organiserId)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.EventDate,
                    e.EventType,
                    e.BannerUrl,
                    VenueName = e.Venue!.VenueName,
                    CategoryCount = e.Categories!.Count(),
                    TotalEnrolments = _context.Enrolments
                        .Count(en => en.Category!.EventID == e.EventID && en.Status != "Cancelled")
                })
                .ToListAsync();

            return Ok(new
            {
                TotalEvents = events.Count,
                TotalEnrolments = events.Sum(e => e.TotalEnrolments),
                UpcomingEvents = events.Where(e => e.EventDate >= DateTime.Now).ToList(),
                Events = events
            });
        }
    }
}