using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayAPI.Filters
{
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        // Pass no roles to just require "logged in, any role"
        public RequireRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserID");
            var role = context.HttpContext.Session.GetString("Role");

            if (userId == null || role == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in to access this resource." });
                return;
            }

            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = $"This action requires one of the following roles: {string.Join(", ", _allowedRoles)}." })
                {
                    StatusCode = 403
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}