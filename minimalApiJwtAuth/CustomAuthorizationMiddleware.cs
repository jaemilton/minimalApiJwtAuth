using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Linq;


namespace MinimalApiJwtAuth;

public sealed class CustomAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public CustomAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool hadJwt = false;

        // 1. If no Authorization header, check for x-username header and accept it.
        //    x-username will be treated as an authenticated principal (no JWT required).
        if (!context.Request.Headers.TryGetValue("Authorization", out var authValues) || string.IsNullOrWhiteSpace(authValues.FirstOrDefault()))
        {
            if (context.Request.Headers.TryGetValue("x-username", out var userValues) && !string.IsNullOrWhiteSpace(userValues.FirstOrDefault()))
            {
                var username = userValues.First();
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "X-Username");
                context.User = new ClaimsPrincipal(identity);
                await _next(context);
                return;
            }

            // No Authorization and no x-username -> not allowed
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Unauthorized: missing credentials");
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        JwtSecurityToken jwt;
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            // token is the raw JWT string
            var handler = new JwtSecurityTokenHandler();
            jwt = handler.ReadJwtToken(token); // JwtSecurityToken
        }
        else {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Unauthorized: invalid Authorization header");
            return;
        }

        jwt.

        hadJwt = true;

        // read issuer from claims
        var iss = jwt.Claims.FirstOrDefault(c => string.Equals(c.Type, "iss", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrWhiteSpace(iss))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Unauthorized: missing iss claim");
            return;
        }

        if (string.Equals(iss, "https://source1", StringComparison.OrdinalIgnoreCase))
        {
            if (!jwt.Claims.Any(c => string.Equals(c.Type, "usr", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(c.Value)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Unauthorized: missing usr claim for source1");
                return;
            }
        }
        else if (string.Equals(iss, "https://source2", StringComparison.OrdinalIgnoreCase))
        {
            if (!jwt.Claims.Any(c => string.Equals(c.Type, "emploeeid", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(c.Value)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Unauthorized: missing emploeeid claim for source2");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Unauthorized: unknown issuer");
            return;
        }

        // Enforce AuthorizeAttribute metadata if present on the endpoint
        var endpoint = context.GetEndpoint();
        if (endpoint is not null)
        {
            var authAttr = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
            if (authAttr is not null)
            {
                // Protected endpoint requires a JWT if you want that behavior.
                // Since x-username is treated as valid here, the code no longer blocks x-username.
                if (!hadJwt)
                {
                    // If you still want protected endpoints to require JWT, change this to return 403 here.
                    // For current request, x-username satisfies authentication so we continue.
                }

                // If issuer is source2, require the specific role(s) declared in the attribute
                if (string.Equals(iss, "https://source2", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(authAttr.Roles))
                {
                    var roles = authAttr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var hasAny = roles.Any(r => HasRoleClaim(context.User, r));
                    if (!hasAny)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync($"Unauthorized: required role(s) {authAttr.Roles} missing");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

    private static bool HasRoleClaim(ClaimsPrincipal user, string role)
    {
        if (user.IsInRole(role))
            return true;

        var roleClaims = user.Claims.Where(c => string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)
            || string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase));

        foreach (var rc in roleClaims)
        {
            var value = rc.Value ?? string.Empty;
            if (value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }
}

