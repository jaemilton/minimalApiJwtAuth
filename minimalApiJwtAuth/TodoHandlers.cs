using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using minimalApiJwtAuth.Authentication;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Security;

namespace MinimalApiJwtAuth;

public static class TodosData
{
    public static Todo[] SampleTodos =
    [
        new(1, "Walk the dog"),
        new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
        new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
        new(4, "Clean the bathroom"),
        new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
    ];
}

public static class TodoHandlers
{
    [HasPermission(headers: "x-usuario")]
    //[CustomAuthorize(headers: "x-username")]
    public static IResult GetTodos()
    {
        return Results.Ok(TodosData.SampleTodos);
    }

    [HasPermission(roles:"X")]
    public static IResult GetTodoById(int id)
    {
        var todo = TodosData.SampleTodos.FirstOrDefault(a => a.Id == id);
        return todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound();
    }
}
