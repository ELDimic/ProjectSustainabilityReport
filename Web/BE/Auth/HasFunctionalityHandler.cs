using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Web.Services;

namespace Web.Auth;

public class HasFunctionalityHandler(IUserService users) : AuthorizationHandler<HasFunctionalityRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasFunctionalityRequirement requirement)
    {
        var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null) return;
        if (await users.HasFunctionalityAsync(id, requirement.Code))
            context.Succeed(requirement);
    }
}