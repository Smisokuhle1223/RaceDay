using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Filters;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/events - public, with optional filters
        [HttpGet]
        public async Task<IActionResult> GetEvents(string? eventType = null, string? search = null, bool upcomingOnly = false)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.EventName.Contains(search) || e.Venue!.City.Contains(search));

            if (upcomingOnly)
                query = query.Where(e => e.EventDate >= DateTime.Now);

            var events = await query
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.EventType,
                    e.BannerUrl,
                    VenueName = e.Venue!.VenueName,
                    City = e.Venue!.City,
                    Province = e.Venue!.Province,
                    OrganiserName = e.Organiser!.FullName
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET /api/events/{id} - public, includes categories
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Where(e => e.EventID == id)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.EventType,
                    e.BannerUrl,
                    VenueName = e.Venue!.VenueName,
                    City = e.Venue!.City,
                    Province = e.Venue!.Province,
                    OrganiserName = e.Organiser!.FullName,
                    Categories = e.Categories!.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName,
                        c.DistanceKM,
                        c.EntryFee,
                        c.MaxParticipants,
                        EnrolledCount = c.Enrolments!.Count(en => en.Status != "Cancelled")
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(ev);
        }

        // POST /api/events - Organiser only
        [RequireRole("Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            var venueExists = await _context.Venues.AnyAsync(v => v.VenueID == dto.VenueID);
            if (!venueExists)
            {
                return BadRequest(new { message = "The specified VenueID does not exist." });
            }

            int organiserId = HttpContext.Session.GetInt32("UserID")!.Value;

            var newEvent = new Event
            {
                EventName = dto.EventName,
                Description = dto.Description,
                EventDate = dto.EventDate,
                EventType = dto.EventType,
                VenueID = dto.VenueID,
                OrganiserID = organiserId,
                CreatedAt = DateTime.Now
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, newEvent);
        }

        // PUT /api/events/{id} - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (ev.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only edit events you created." });
            }

            var venueExists = await _context.Venues.AnyAsync(v => v.VenueID == dto.VenueID);
            if (!venueExists)
            {
                return BadRequest(new { message = "The specified VenueID does not exist." });
            }

            ev.EventName = dto.EventName;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.EventType = dto.EventType;
            ev.VenueID = dto.VenueID;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        // DELETE /api/events/{id} - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (ev.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only delete events you created." });
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted successfully." });
        }
    }
}