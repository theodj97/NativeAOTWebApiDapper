using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models.Requests.Todo;

namespace WebApiDapperNativeAOT.Routes;

public static class TodoRoutes
{
    public static void TodoRoute(WebApplication app)
    {
        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", async (TodoHandler handler, string[]? title, string[]? description, int? createdBy, int[]? assignedTo, bool? isComplete, CancellationToken cancellationToken = default) =>
        {
            var todos = await handler.SearchAsync(title, description, createdBy, assignedTo, isComplete, cancellationToken);
            if (todos.Any())
                return Results.Ok(todos);
            return Results.NoContent();
        });

        todosApi.MapGet("/{id}", async (TodoHandler handler, int id, CancellationToken cancellationToken = default) => await handler.GetByIdAsync(id, cancellationToken) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());

        todosApi.MapPost("/", async (TodoHandler handler, TodoCreateRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.CreateAsync(request, cancellationToken);
            return Results.Created(string.Empty, result);
        });

        todosApi.MapPut("/{id}", async (TodoHandler handler, int id, TodoUpdateRequest request, CancellationToken cancellationToken = default) => await handler.UpdateAsync(id, request, cancellationToken));

        todosApi.MapDelete("/{id}", async (TodoHandler handler, int id, CancellationToken cancellationToken = default) => await handler.DeleteAsync(id, cancellationToken));
    }
}
