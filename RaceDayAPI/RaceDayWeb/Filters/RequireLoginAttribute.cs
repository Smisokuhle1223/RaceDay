using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayWeb.Filters
{
    // Protects MVC pages. Not logged in -> redirected to login.
    // Wrong role -> redirected to an access denied page.
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RequireLoginAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (role == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}