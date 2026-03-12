using System;
using Microsoft.AspNetCore.Authorization;

namespace MinimalApiJwtAuth
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CustomAuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
        /// </summary>
        public string? Roles { get; set; }

        public CustomAuthorizeAttribute()
        {

        }
    }
}
