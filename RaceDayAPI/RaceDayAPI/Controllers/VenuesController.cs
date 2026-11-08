using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public VenuesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/venues - public, used for the event location dropdown
        [HttpGet("api/venues")]
        public async Task<IActionResult> GetVenues()
        {
            var venues = await _context.Venues
                .OrderBy(v => v.VenueName)
                .Select(v => new { v.VenueID, v.VenueName, v.City, v.Province })
                .ToListAsync();

            return Ok(venues);
        }
    }
}