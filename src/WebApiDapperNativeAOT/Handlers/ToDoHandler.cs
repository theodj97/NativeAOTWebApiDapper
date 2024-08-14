using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using WebApiDapperNativeAOT.Handlers.Mappers;
using WebApiDapperNativeAOT.Models.Entities;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;

namespace WebApiDapperNativeAOT.Handlers;

public class TodoHandler(string connectionString)
{
    private readonly string connectionString = connectionString;

    [DapperAot]
    public async Task<IEnumerable<TodoResponse>> SearchAsync(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, bool? isComplete = null)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var query = new StringBuilder("SELECT Id, Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete FROM dbo.Todos");

        List<string> conditions = [];

        if (title?.Length > 0)
            conditions.Add($"Title IN ({string.Join(",", title.Select(t => $"'{t.Replace("'", "''")}'"))})");

        if (description?.Length > 0)
            conditions.Add($"Description IN ({string.Join(",", description.Select(d => $"'{d.Replace("'", "''")}'"))})");

        if (createdBy.HasValue)
            conditions.Add("CreatedBy = @createdBy");

        if (assignedTo?.Length > 0)
            conditions.Add($"AssignedTo = '{string.Join(",", assignedTo)}'");

        if (isComplete.HasValue)
            conditions.Add("IsComplete = @isComplete");

        if (conditions.Count > 0)
            query.Append(" WHERE ").Append(string.Join(" AND ", conditions));

        var parameters = new { createdBy, isComplete };
        var response = await connection.QueryAsync<TodoEntity>(query.ToString(), parameters);
        return TodoMapper.FromEntityToResponse(response);
    }

    [DapperAot]
    public async Task<TodoResponse> GetByIdAsync(int todoId)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var response = await connection.QueryFirstAsync<TodoEntity>("select Id, Title, Description, CreatedBy, AssignedTo,TargetDate, IsComplete from dbo.Todos where Id=@todoId", new { todoId });
        return TodoMapper.FromEntityToResponse(response);
    }

    [DapperAot]
    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var query = @"INSERT INTO dbo.Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) 
        VALUES (@Title, @Description, @CreatedBy, @AssignedTo, @TargetDate, @IsComplete);
        SELECT CAST(SCOPE_IDENTITY() as int);";

        var newId = await connection.QuerySingleAsync<int>(query, request);
        return TodoMapper.FromCreateRequestToResponse(request, newId);
    }

    [DapperAot]
    public async Task<bool> UpdateAsync(int id, TodoUpdateRequest request)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var query = "UPDATE dbo.Todos SET Title = @Title, Description = @Description, CreatedBy = @CreatedBy, AssignedTo = @AssignedTo, TargetDate = @TargetDate, IsComplete = @IsComplete WHERE Id = @Id";
        var parameters = new
        {
            request.Title,
            request.Description,
            request.CreatedBy,
            request.AssignedTo,
            request.TargetDate,
            request.IsComplete,
            Id = id
        };
        var result = await connection.ExecuteAsync(query, parameters);
        return result > 0;
    }

    [DapperAot]
    public async Task DeleteAsync(int todoId)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var query = "DELETE FROM dbo.Todos WHERE Id = @todoId";
        await connection.ExecuteAsync(query, new { todoId });
    }
}
