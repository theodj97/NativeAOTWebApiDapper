using Microsoft.AspNetCore.Mvc;
using WebApiDapperNativeAOT.Handlers.Todo;
using WebApiDapperNativeAOT.Models.Requests.Todo;

namespace WebApiDapperNativeAOT.Routes;

public static class TodoRoutes
{
    public static void TodoRoute(WebApplication app)
    {
        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", async (TodoHandler handler, [FromQuery] string[]? title, [FromQuery] string[]? description, [FromQuery] int? createdBy, [FromQuery] int[]? assignedTo, [FromQuery] DateTime[]? targetDate, [FromQuery] bool? isComplete, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.SearchAsync(new TodoSearchRequest(title, description, createdBy, assignedTo, targetDate, isComplete), cancellationToken);
            return result.ToResult();
        });

        todosApi.MapGet("/{id}", async (TodoHandler handler, int id, CancellationToken cancellationToken = default) => (await handler.GetByIdAsync(id, cancellationToken)).ToResult());

        todosApi.MapPost("/", async (TodoHandler handler, [FromBody] TodoCreateRequest request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.CreateAsync(request, cancellationToken);
            return result.ToResult();
        });

        todosApi.MapPut("/{id}", async (TodoHandler handler, int id, [FromBody] TodoUpdateRequest request, CancellationToken cancellationToken = default) => (await handler.UpdateAsync(id, request, cancellationToken)).ToResult());

        todosApi.MapDelete("/{id}", async (TodoHandler handler, int id, CancellationToken cancellationToken = default) => await handler.DeleteAsync(id, cancellationToken));

        todosApi.MapPost("/bulkInsert", async (TodoHandler handler, [FromBody] IEnumerable<TodoCreateRequest> request, CancellationToken cancellationToken = default) =>
        {
            var result = await handler.BulkInsertAsync(request, cancellationToken);
            return result.ToResult();
        });

        todosApi.MapPut("/bulkUpdate", async (TodoHandler handler, [FromBody] IEnumerable<TodoBulkUpdateRequest> request, CancellationToken cancellationToken = default) => (await handler.BulkUpdateAsync(request, cancellationToken)).ToResult());

        todosApi.MapDelete("/bulkDelete", async (TodoHandler handler, [FromBody] IEnumerable<int> request, CancellationToken cancellationToken = default) => (await handler.BulkDeleteAsync(request, cancellationToken)).ToResult());
    }
}
