using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

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
    [CustomAuthorize(Roles = "GETALL")]
    public static IResult GetTodos()
    {
        return Results.Ok(TodosData.SampleTodos);
    }

    public static IResult GetTodoById(int id)
    {
        var todo = TodosData.SampleTodos.FirstOrDefault(a => a.Id == id);
        return todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound();
    }
}
