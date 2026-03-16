using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Net;

namespace minimalApiJwtAuth.Authentication
{
    public class CustomAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // If authorization failed and the user is authenticated (so it's a 403 scenario)
            if (authorizeResult.Forbidden && context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                // Optional: add a custom message or response body for APIs
                // await context.Response.WriteAsync("You do not have the necessary permissions."); 
                return; // Stop further handling by the default handler
            }

            // Fall back to the default handler for other cases (e.g., 401 for unauthenticated)
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
