using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using minimalApiJwtAuth.Authentication;
using MinimalApiJwtAuth;
//using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
var validIssuers = new List<string>() { "issuer1", "issuer1" };


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

//// enable authorization (and authentication if you will use JwtBearer)
//builder.Services.AddAuthorization();

//// If you plan to validate JWTs in production, configure authentication here:
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = "https://source1", // set appropriately or from config
//            ValidateAudience = false,
//            ValidateIssuerSigningKey = false,
//            ValidateLifetime = true,
//            // Configure signing key, issuer signing key, etc.
//        };
//    });


//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer();



//Other code
//builder.Services.AddSingleton<IAuthorizationFilter, CustomAuthorizeAttribute>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure token validation parameters (Issuer, Audience, SigningKey, etc.)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            // 2. Define custom validator with regex
            IssuerValidator = (issuer, securityToken, validationParameters) =>
            {
                //// Example: Match https://tenant1.auth.com or https://tenant2.auth.com
                //var regex = new Regex(@"^https:\/\/[a-zA-Z0-9]+\.auth\.com$");

                //if (regex.IsMatch(issuer))
                if (validIssuers.Contains(issuer))
                {
                    return issuer; // Valid
                }

                throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
            },
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false,       // Allows unsigned tokens
            // Bypasses signature validation logic
            SignatureValidator = (token, parameters) => {
                return new JsonWebToken(token);
            }
        };

        // Add event handler for debug
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully.");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine(
                    "Message received. Authorization header: " + authHeader);

                return Task.CompletedTask;
            },
            
            //OnRedirectToAccessDenied = context =>
            //{
            //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //    return context.Response.CompleteAsync();
            //}
        };

    });


builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationResultHandler>();
builder.Services.AddOpenApi();

// Build app...
var app = builder.Build();

// Ensure authentication middleware runs before authorization
//app.UseMiddleware<CustomAuthorizationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", TodoHandlers.GetTodos)
        .WithName("GetTodos");

todosApi.MapGet("/{id}", TodoHandlers.GetTodoById)
    .WithName("GetTodoById");

// app.Run();
app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
