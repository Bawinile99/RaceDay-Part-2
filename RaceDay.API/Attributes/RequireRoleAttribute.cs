using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Services;

namespace RaceDay.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Not authenticated." });
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(session.GetRole()))
        {
            context.Result = new ObjectResult(new { message = "Forbidden." }) { StatusCode = 403 };
        }
    }
}
