using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace minimalApiJwtAuth.Authentication;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = (HttpContext)(context.Resource);
        // 1. If no Authorization header, check for x-username header and accept it.
        //    x-username will be treated as an authenticated principal (no JWT required).
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authValues) || string.IsNullOrWhiteSpace(authValues.FirstOrDefault()))
        {
            if (httpContext.Request.Headers.TryGetValue("x-username", out var userValues) && !string.IsNullOrWhiteSpace(userValues.FirstOrDefault()))
            {
                var username = userValues.First();
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "X-Username");
                httpContext.User = new ClaimsPrincipal(identity);
                context.Succeed(requirement);
               
            }

            // No Authorization and no x-username -> not allowed
            context.Fail();
            
        }


        //using IServiceScope scope = _serviceScopeFactory.CreateScope();

        //IPermissionService permissionService = scope.ServiceProvider
        //    .GetRequiredService<IPermissionService>();

        //HashSet<string> permissions = await permissionService
        //    .GetPermissionsAsync(parsedMemberId);

        //if (permissions.Contains(requirement.Permission))
        //{
        //    context.Succeed(requirement);
        //}
        
    }
}
