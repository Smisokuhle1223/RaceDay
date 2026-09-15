using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class EnrolmentsControllerTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private EnrolmentsController GetControllerAsUser(RaceDayDbContext context, int userId, string role)
        {
            var controller = new EnrolmentsController(context);

            var session = new TestSession();
            session.SetInt32("UserID", userId);
            session.SetString("Role", role);

            var httpContext = new DefaultHttpContext { Session = session };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        // Sets up a basic Organiser, Venue, Event, and Category
        // that a Participant can then try to enrol in.
        private async Task<RaceDayDbContext> SeedEventAndCategory()
        {
            var context = GetInMemoryContext();

            context.Users.Add(new User { UserID = 1, FullName = "Organiser", Email = "org@test.com", PasswordHash = "x", Role = "Organiser" });
            context.Users.Add(new User { UserID = 2, FullName = "Participant", Email = "part@test.com", PasswordHash = "x", Role = "Participant" });
            context.Venues.Add(new Venue { VenueID = 1, VenueName = "Test Venue", City = "Durban", Province = "KZN" });
            context.Events.Add(new Event { EventID = 1, EventName = "Test Event", EventDate = DateTime.Now, VenueID = 1, OrganiserID = 1 });
            context.Categories.Add(new Category { CategoryID = 1, EventID = 1, CategoryName = "5km", DistanceKM = 5, MaxParticipants = 100, EntryFee = 0 });

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreatedAndIsRecorded()
        {
            // Arrange
            var context = await SeedEventAndCategory();
            var controller = GetControllerAsUser(context, 2, "Participant");

            // Act
            var result = await controller.Enrol(1);

            // Assert - correct response
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);

            // Assert - actually recorded in the database, linked to the right participant and category
            var savedEnrolment = await context.Enrolments.FirstOrDefaultAsync(e => e.ParticipantID == 2 && e.CategoryID == 1);
            Assert.NotNull(savedEnrolment);
            Assert.Equal("Confirmed", savedEnrolment!.Status);
        }


        [Fact]
        public async Task Enrol_WhenAlreadyEnrolled_ReturnsConflict()
        {
            // Arrange
            var context = await SeedEventAndCategory();
            var controller = GetControllerAsUser(context, 2, "Participant");

            // Act - enrol twice
            await controller.Enrol(1);
            var secondResult = await controller.Enrol(1);

            // Assert
            Assert.IsType<ConflictObjectResult>(secondResult);
        }
    }
}