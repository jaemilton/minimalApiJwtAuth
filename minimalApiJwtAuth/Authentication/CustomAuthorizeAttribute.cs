using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MinimalApiJwtAuth;
using System.Data;
using System.Reflection.PortableExecutable;

namespace minimalApiJwtAuth.Authentication
{
    // Define where the attribute can be used (classes and/or methods)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {

        public CustomAuthorizeAttribute(string? roles = null, string? headers = null, PermissionRequirementTypeEnum type = PermissionRequirementTypeEnum.OneOf)
        {
            if (roles == null && headers == null)
                throw new ArgumentNullException("roles or headers", "atLeast one of them must be set");

            Roles = roles;
            Headers = headers;

        }

        public PermissionRequirementTypeEnum Type { get; }

        public string? Roles { get; }

        public string? Headers { get; }


        private IEnumerable<string>? _rolesArray = null;
        private IEnumerable<string> RolesArray
        {
            get
            {
                if (_rolesArray == null)
                {
                    _rolesArray = Roles?.Split(',').Select(r => r.Trim()).ToArray() ?? Array.Empty<string>();
                }
                return _rolesArray;
            }
        }


        private IEnumerable<string>? _headersArray = null;
        private IEnumerable<string> HeadersArray
        {
            get
            {
                if (_headersArray == null)
                {
                    _headersArray = Headers?.Split(',').Select(r => r.Trim()).ToArray() ?? Array.Empty<string>();
                }
                return _headersArray;
            }
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {

            // 1. If no Authorization header, check for x-username header and accept it.
            //    x-username will be treated as an authenticated principal (no JWT required).

            var headers = context.HttpContext.Request.Headers;
            if (Type == PermissionRequirementTypeEnum.OneOf)
            {
                bool flowControl = OneOfAuthorizationValidation(context.HttpContext, headers);
                if (!flowControl)
                {
                    return;
                }

            }
            else
            {
                bool flowControl = AllOfAuthorizationValidation(context.HttpContext, headers);
                if (!flowControl)
                {
                    return;
                }

            }

            // No Authorization and no necessery Headers present -> not allowed
            // Set the result to 403 Forbidden if they don't have the permission
            context.Result = new ForbidResult();


        }

        // A placeholder for your custom permission checking method
        private bool CheckUserPermission(System.Security.Claims.ClaimsPrincipal user, string permission)
        {
            // **This is where you implement your specific logic.**
            // E.g., check against a list of permissions stored in claims or a database.
            // For demonstration, let's assume a claim "Permission" must exist with the value.
            var hasClaim = user.HasClaim("Permission", permission);
            return hasClaim;
        }


        private bool AllOfAuthorizationValidation(HttpContext context, IHeaderDictionary headers)
        {
            bool headersValid = true;
            foreach (var header in RolesArray)
            {
                if (!headers.TryGetValue(header, out var headerValues) || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
                {
                    headersValid = false;
                    break;
                }
            }
            bool rolesValid = true;
            if (headersValid)
            {
                foreach (var role in RolesArray)
                {
                    if (!context.User.IsInRole(role))
                    {
                        rolesValid = false;
                        break;
                    }
                }
            }

            if (headersValid && rolesValid)
            {
                return false;
            }

            return true;
        }

        private bool OneOfAuthorizationValidation(HttpContext context, IHeaderDictionary headers)
        {
            string authorizationHeader = headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                //check if one of requirement.Headers are present and non-empty
                foreach (var header in HeadersArray)
                {
                    if (headers.TryGetValue(header, out var headerValues) && !string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (var role in RolesArray)
                {
                    if (context.User.IsInRole(role))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
