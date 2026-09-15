using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class EventsControllerTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // Helper: builds an EventsController with a fake session
        // already logged in as a given user/role.
        private EventsController GetControllerAsUser(RaceDayDbContext context, int userId, string role)
        {
            var controller = new EventsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", userId);
            session.SetString("Role", role);

            var httpContext = new DefaultHttpContext { Session = session };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Users.Add(new User { UserID = 1, FullName = "Organiser", Email = "org@test.com", PasswordHash = "x", Role = "Organiser" });
            context.Venues.Add(new Venue { VenueID = 1, VenueName = "Test Venue", City = "Durban", Province = "KZN" });
            await context.SaveChangesAsync();

            var controller = GetControllerAsUser(context, 1, "Organiser");
            var dto = new CreateEventDto
            {
                EventName = "Test Event",
                EventDate = DateTime.Now.AddMonths(1),
                VenueID = 1
            };

            // Act
            var result = await controller.CreateEvent(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_AsNonOwningOrganiser_ReturnsForbidden()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Users.Add(new User { UserID = 1, FullName = "Owner", Email = "owner@test.com", PasswordHash = "x", Role = "Organiser" });
            context.Users.Add(new User { UserID = 2, FullName = "OtherOrganiser", Email = "other@test.com", PasswordHash = "x", Role = "Organiser" });
            context.Venues.Add(new Venue { VenueID = 1, VenueName = "Test Venue", City = "Durban", Province = "KZN" });
            context.Events.Add(new Event { EventID = 1, EventName = "Existing Event", EventDate = DateTime.Now, VenueID = 1, OrganiserID = 1 });
            await context.SaveChangesAsync();

            // Logged in as UserID 2, but the event belongs to UserID 1
            var controller = GetControllerAsUser(context, 2, "Organiser");
            var dto = new UpdateEventDto
            {
                EventName = "Hijacked Event",
                EventDate = DateTime.Now.AddMonths(1),
                VenueID = 1
            };

            // Act
            var result = await controller.UpdateEvent(1, dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetEvents_ReturnsOkWithoutRequiringLogin()
        {
            // Arrange - no session set up at all, simulating an anonymous visitor
            var context = GetInMemoryContext();
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetEvents();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}