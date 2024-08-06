using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using WebApiDapperNativeAOT.Models;

namespace WebApiDapperNativeAOT.Handlers;

public class ToDoHandler(string connectionString)
{
    private readonly string connectionString = connectionString;

    [DapperAot]
    public Todo[] Search(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, bool? isComplete = null)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
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
        var response = connection.Query<Todo>(query.ToString(), parameters);
        return response.ToArray();
    }

    [DapperAot]
    public Todo GetById(int todoId)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        var response = connection.QueryFirst<Todo>("select Id, Title, Description, CreatedBy, AssignedTo,TargetDate, IsComplete from dbo.Todos where Id=@todoId", new { todoId });
        return response;
    }

    [DapperAot]
    public void Create(Todo todo)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        var query = "INSERT INTO dbo.Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) VALUES (@Title, @Description, @CreatedBy, @AssignedTo, @TargetDate, @IsComplete)";
        connection.Execute(query, todo);
    }
}
