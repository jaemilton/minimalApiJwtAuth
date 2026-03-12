using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApiJwtAuth;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

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

builder.Services.AddOpenApi();

// Build app...
var app = builder.Build();

// Ensure authentication middleware runs before authorization
//app.UseAuthentication();
app.UseMiddleware<CustomAuthorizationMiddleware>();
//app.UseAuthorization();

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
