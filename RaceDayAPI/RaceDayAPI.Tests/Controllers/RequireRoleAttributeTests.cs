using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDayAPI.Filters;
using Xunit;

namespace RaceDayAPI.Tests.Controllers
{
    public class RequireRoleAttributeTests
    {
        // Builds a minimal ActionExecutingContext, with a given (or absent)
        // session, so we can test the filter's logic in isolation.
        private ActionExecutingContext BuildContext(int? userId, string? role)
        {
            var session = new TestSession();
            if (userId.HasValue) session.SetInt32("UserID", userId.Value);
            if (role != null) session.SetString("Role", role);

            var httpContext = new DefaultHttpContext { Session = session };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: new object());
        }

        [Fact]
        public void OnActionExecuting_NotLoggedIn_ReturnsUnauthorized()
        {
            // Arrange
            var filter = new RequireRoleAttribute("Participant");
            var context = BuildContext(userId: null, role: null);

            // Act
            filter.OnActionExecuting(context);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void OnActionExecuting_WrongRole_ReturnsForbidden()
        {
            // Arrange - logged in as an Organiser, but this action requires Participant
            var filter = new RequireRoleAttribute("Participant");
            var context = BuildContext(userId: 1, role: "Organiser");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public void OnActionExecuting_CorrectRole_AllowsRequestThrough()
        {
            // Arrange
            var filter = new RequireRoleAttribute("Participant");
            var context = BuildContext(userId: 2, role: "Participant");

            // Act
            filter.OnActionExecuting(context);

            // Assert - context.Result stays null, meaning the filter didn't block anything
            Assert.Null(context.Result);
        }
    }
}