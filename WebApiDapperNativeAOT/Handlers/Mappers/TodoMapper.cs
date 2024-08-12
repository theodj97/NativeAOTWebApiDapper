using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Models;
using WebApiDapperNativeAOT.Models.Requests.Todo;

namespace WebApiDapperNativeAOT.Handlers.Mappers;

public static class TodoMapper
{
    public static TodoResponse FromEntityToResponse(TodoEntity entity)
    {
        return new TodoResponse(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.CreatedBy,
            entity.AssignedTo,
            entity.TargetDate,
            entity.IsComplete
        );
    }

    public static IEnumerable<TodoResponse> FromEntityToResponse(IEnumerable<TodoEntity> entities)
    {
        return entities.Select(FromEntityToResponse);
    }

    public static TodoResponse FromCreateRequestToResponse(TodoCreateRequest request, int id)
    {
        return new TodoResponse(
         id,
         request.Title,
         request.Description,
         request.CreatedBy,
         request.AssignedTo,
         request.TargetDate,
         request.IsComplete
     );
    }
}
