using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models.Requests.Todo;

namespace WebApiDapperNativeAOT.Routes;

public static class TodoRoutes
{
    public static void TodoRoute(WebApplication app)
    {
        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", async (TodoHandler handler, string[]? title, string[]? description, int? createdBy, int[]? assignedTo, bool? isComplete) =>
        {
            var todos = await handler.SearchAsync(title, description, createdBy, assignedTo, isComplete);
            if (todos.Any())
                return Results.Ok(todos);
            return Results.NoContent();
        });

        todosApi.MapGet("/{id}", async (TodoHandler handler, int id) => await handler.GetByIdAsync(id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());

        todosApi.MapPost("/", async (TodoHandler handler, TodoCreateRequest request) =>
        {
            var result = await handler.CreateAsync(request);
            return Results.Created(string.Empty, result);
        });

        todosApi.MapPut("/{id}", async (TodoHandler handler, int id, TodoUpdateRequest request) => await handler.UpdateAsync(id, request));

        todosApi.MapDelete("/{id}", async (TodoHandler handler, int id) => await handler.DeleteAsync(id));
    }
}
