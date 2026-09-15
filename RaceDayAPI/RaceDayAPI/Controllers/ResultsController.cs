using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Filters;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST /api/enrolments/{id}/results - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpPost("api/enrolments/{enrolmentId}/results")]
        public async Task<IActionResult> CaptureResult(int enrolmentId, ResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (enrolment.Category!.Event!.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only capture results for events you created." });
            }

            bool resultExists = await _context.Results.AnyAsync(r => r.EnrolmentID == enrolmentId);
            if (resultExists)
            {
                return Conflict(new { message = "A result has already been captured for this enrolment." });
            }

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = dto.Status,
                RecordedAt = DateTime.Now
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, result);
        }

        // GET /api/enrolments/{id}/results - owning participant or event organiser
        [RequireRole("Organiser", "Participant")]
        [HttpGet("api/enrolments/{enrolmentId}/results")]
        public async Task<IActionResult> GetResultForEnrolment(int enrolmentId)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            string currentRole = HttpContext.Session.GetString("Role")!;

            bool isOwningParticipant = enrolment.ParticipantID == currentUserId;
            bool isOwningOrganiser = currentRole == "Organiser" && enrolment.Category!.Event!.OrganiserID == currentUserId;

            if (!isOwningParticipant && !isOwningOrganiser)
            {
                return StatusCode(403, new { message = "You are not authorised to view this result." });
            }

            var result = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);
            if (result == null)
            {
                return NotFound(new { message = "No result has been captured for this enrolment yet." });
            }

            return Ok(result);
        }

        // GET /api/users/{id}/results - owning participant or any organiser
        [RequireRole("Organiser", "Participant")]
        [HttpGet("api/users/{userId}/results")]
        public async Task<IActionResult> GetResultsForUser(int userId)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            string currentRole = HttpContext.Session.GetString("Role")!;

            if (currentRole == "Participant" && userId != currentUserId)
            {
                return StatusCode(403, new { message = "You can only view your own results." });
            }

            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.ParticipantID == userId)
                .Select(r => new
                {
                    r.ResultID,
                    r.FinishTime,
                    r.Position,
                    r.Status,
                    EventName = r.Enrolment!.Category!.Event!.EventName,
                    CategoryName = r.Enrolment!.Category!.CategoryName
                })
                .ToListAsync();

            return Ok(results);
        }

        // PUT /api/results/{id} - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpPut("api/results/{id}")]
        public async Task<IActionResult> UpdateResult(int id, ResultDto dto)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new { message = "Result not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (result.Enrolment!.Category!.Event!.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only update results for events you created." });
            }

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            result.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(result);
        }
    }
}