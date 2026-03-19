using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace minimalApiJwtAuth.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class HasPermissionAttribute : AuthorizeAttribute, IAuthorizeData
    {
        // Static registry mapping policy names to runtime PermissionRequirement instances.
        // Using an initialized property avoids null reference issues.
        public static Dictionary<string, PermissionRequirement> Permissions { get; } = new();

        // Constructor uses only attribute-legal parameter types (string).
        // 'policy' serves as the key for looking up the created PermissionRequirement at runtime.
        public HasPermissionAttribute
            (string? headers = null, 
            string? roles = null,  
            PermissionRequirementTypeEnum type = PermissionRequirementTypeEnum.OneOf): base(policy: Guid.NewGuid().ToString()) // Policy will be set to the provided 'policy' parameter below.
        {
            if (string.IsNullOrWhiteSpace(Policy))
                throw new ArgumentException("Policy name must be provided", nameof(Policy));

            // Create the PermissionRequirement at runtime and store it in the static dictionary.
            // This avoids passing complex objects as attribute constructor parameters.
            Permissions.Add(Policy, new PermissionRequirement(headers: headers, roles: roles, type: type));
        }
    }
}
