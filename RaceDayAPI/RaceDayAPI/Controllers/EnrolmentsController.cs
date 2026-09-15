using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Filters;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST /api/categories/{id}/enrolments - Participant only
        [RequireRole("Participant")]
        [HttpPost("api/categories/{categoryId}/enrolments")]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            int participantId = HttpContext.Session.GetInt32("UserID")!.Value;

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantID == participantId && e.CategoryID == categoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            int currentEnrolmentCount = await _context.Enrolments
                .CountAsync(e => e.CategoryID == categoryId && e.Status != "Cancelled");

            if (currentEnrolmentCount >= category.MaxParticipants)
            {
                return BadRequest(new { message = "This category has reached its maximum number of participants." });
            }

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = categoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, enrolment);
        }

        // GET /api/enrolments/my - Participant only, their own history
        [RequireRole("Participant")]
        [HttpGet("api/enrolments/my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            int participantId = HttpContext.Session.GetInt32("UserID")!.Value;

            var enrolments = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .Where(e => e.ParticipantID == participantId)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.Status,
                    e.EnrolmentDate,
                    CategoryName = e.Category!.CategoryName,
                    EventName = e.Category!.Event!.EventName
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET /api/events/{id}/enrolments - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpGet("api/events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (ev.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only view enrolments for events you created." });
            }

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.Category!.EventID == eventId)
                .Select(e => new
                {
                    e.EnrolmentID,
                    ParticipantName = e.Participant!.FullName,
                    CategoryName = e.Category!.CategoryName,
                    e.Status,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE /api/enrolments/{id} - Participant only, must own the enrolment
        [RequireRole("Participant")]
        [HttpDelete("api/enrolments/{id}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (enrolment.ParticipantID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only cancel your own enrolments." });
            }

            enrolment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled successfully." });
        }
    }
}