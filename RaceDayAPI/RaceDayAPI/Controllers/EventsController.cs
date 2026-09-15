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

        // GET /api/events - public, anyone can browse
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organiser)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    VenueName = e.Venue!.VenueName,
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
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(new
            {
                ev.EventID,
                ev.EventName,
                ev.Description,
                ev.EventDate,
                Venue = ev.Venue!.VenueName,
                Categories = ev.Categories!.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.DistanceKM,
                    c.EntryFee
                })
            });
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
                VenueID = dto.VenueID,
                OrganiserID = organiserId,
                CreatedAt = DateTime.Now
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, newEvent);
        }

        // PUT /api/events/{id} - Organiser only, and must own the event
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
            ev.VenueID = dto.VenueID;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        // DELETE /api/events/{id} - Organiser only, and must own the event
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