using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class EventsFilterTests
    {
        private async Task<RaceDayDbContext> SeedEvents()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);

            context.Users.Add(new User { UserID = 1, FullName = "Organiser", Email = "o@test.com", PasswordHash = "x", Role = "Organiser" });
            context.Venues.Add(new Venue { VenueID = 1, VenueName = "Venue", City = "Durban", Province = "KZN" });
            context.Events.Add(new Event { EventID = 1, EventName = "City Run", EventType = "Run", EventDate = DateTime.Now.AddDays(10), VenueID = 1, OrganiserID = 1 });
            context.Events.Add(new Event { EventID = 2, EventName = "Cycle Tour", EventType = "Cycle", EventDate = DateTime.Now.AddDays(20), VenueID = 1, OrganiserID = 1 });
            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task GetEvents_WithEventTypeFilter_ReturnsOnlyMatchingEvents()
        {
            var controller = new EventsController(await SeedEvents());
            var result = await controller.GetEvents("Cycle", null, false);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Single(((System.Collections.IEnumerable)ok.Value!).Cast<object>());
        }

        [Fact]
        public async Task GetEvents_WithNoFilter_ReturnsAllEvents()
        {
            var controller = new EventsController(await SeedEvents());
            var result = await controller.GetEvents(null, null, false);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, ((System.Collections.IEnumerable)ok.Value!).Cast<object>().Count());
        }
    }
}
