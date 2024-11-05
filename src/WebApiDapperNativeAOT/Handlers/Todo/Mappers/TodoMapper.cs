using Microsoft.Data.SqlClient;
using WebApiDapperNativeAOT.Models.Entities;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;

namespace WebApiDapperNativeAOT.Handlers.Todo.Mappers;

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

    public static IEnumerable<TodoResponse> FromEntitiesToResponse(IEnumerable<TodoEntity> entities)
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

    public static TodoEntity MapReaderToTodoEntity(SqlDataReader reader)
    {
        return new TodoEntity
        (
            Id: reader.GetInt32(0),
            Title: reader.GetString(1),
            Description: reader.IsDBNull(2) ? null : reader.GetString(2),
            CreatedBy: reader.GetInt32(3),
            AssignedTo: reader.IsDBNull(4) ? null : reader.GetString(4),
            TargetDate: reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            IsComplete: reader.GetBoolean(6)
        );
    }
}
