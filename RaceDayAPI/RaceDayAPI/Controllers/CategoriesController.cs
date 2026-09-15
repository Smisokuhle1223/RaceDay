using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Filters;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/events/{id}/categories - public
        [HttpGet("api/events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST /api/events/{id}/categories - Organiser only, must own the event
        [RequireRole("Organiser")]
        [HttpPost("api/events/{eventId}/categories")]
        public async Task<IActionResult> AddCategory(int eventId, CategoryDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (ev.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only add categories to events you created." });
            }

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName,
                DistanceKM = dto.DistanceKM,
                MaxParticipants = dto.MaxParticipants,
                EntryFee = dto.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, category);
        }

        // PUT /api/categories/{id} - Organiser only, must own the parent event
        [RequireRole("Organiser")]
        [HttpPut("api/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (category.Event!.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only edit categories for events you created." });
            }

            category.CategoryName = dto.CategoryName;
            category.DistanceKM = dto.DistanceKM;
            category.MaxParticipants = dto.MaxParticipants;
            category.EntryFee = dto.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE /api/categories/{id} - Organiser only, must own the parent event
        [RequireRole("Organiser")]
        [HttpDelete("api/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID")!.Value;
            if (category.Event!.OrganiserID != currentUserId)
            {
                return StatusCode(403, new { message = "You can only delete categories for events you created." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}