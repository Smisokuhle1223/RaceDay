using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Filters;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/users/profile - any logged-in user, their own profile
        [RequireRole("Organiser", "Participant")]
        [HttpGet("api/users/profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            int userId = HttpContext.Session.GetInt32("UserID")!.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                user.CreatedAt
            });
        }

        // PUT /api/users/profile - any logged-in user, updates their own profile
        [RequireRole("Organiser", "Participant")]
        [HttpPut("api/users/profile")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            int userId = HttpContext.Session.GetInt32("UserID")!.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserID != userId);
            if (emailTaken)
            {
                return Conflict(new { message = "That email is already in use by another account." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role
            });
        }

        // GET /api/users/{id} - public profile lookup (e.g. organiser name on an event listing)
        [HttpGet("api/users/{id}")]
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Role
            });
        }
    }
}