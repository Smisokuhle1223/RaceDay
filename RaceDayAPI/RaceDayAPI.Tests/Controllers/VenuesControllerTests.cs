using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class VenuesControllerTests
    {
        [Fact]
        public async Task GetVenues_ReturnsAllVenues_WithoutLogin()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);
            context.Venues.Add(new Venue { VenueID = 1, VenueName = "Venue A", City = "Durban", Province = "KZN" });
            context.Venues.Add(new Venue { VenueID = 2, VenueName = "Venue B", City = "Cape Town", Province = "WC" });
            await context.SaveChangesAsync();

            var controller = new VenuesController(context);
            var result = await controller.GetVenues();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, ((System.Collections.IEnumerable)ok.Value!).Cast<object>().Count());
        }
    }
}
