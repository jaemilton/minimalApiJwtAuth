using Microsoft.AspNetCore.Authorization;

namespace minimalApiJwtAuth.Authentication;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string? roles = null, string? headers = null, PermissionRequirementTypeEnum type = PermissionRequirementTypeEnum.OneOf)
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
}
