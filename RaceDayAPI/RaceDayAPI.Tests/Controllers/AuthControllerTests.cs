using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        // Creates a fresh, isolated in-memory database for each test,
        // so tests never interfere with each other or with the real database.
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "unittest@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "unittest2@raceday.co.za",
                Password = "Password123",
                Role = "Administrator" // not a valid role
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "duplicate@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            };

            // Act - register the same email twice
            await controller.Register(dto);
            var secondResult = await controller.Register(dto);

            // Assert
            Assert.IsType<ConflictObjectResult>(secondResult);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOk()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new AuthController(context);
            var registerDto = new RegisterDto
            {
                FullName = "Login Test",
                Email = "logintest@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            };
            await controller.Register(registerDto);

            // Give the controller a fake HTTP context so Session works in the test
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var loginDto = new LoginDto
            {
                Email = "logintest@raceday.co.za",
                Password = "Password123"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new AuthController(context);
            var registerDto = new RegisterDto
            {
                FullName = "Login Test 2",
                Email = "logintest2@raceday.co.za",
                Password = "Password123",
                Role = "Participant"
            };
            await controller.Register(registerDto);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var loginDto = new LoginDto
            {
                Email = "logintest2@raceday.co.za",
                Password = "WrongPassword"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}