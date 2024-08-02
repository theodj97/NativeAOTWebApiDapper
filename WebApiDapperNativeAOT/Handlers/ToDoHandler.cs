using Dapper;
using Microsoft.Data.SqlClient;
using WebApiDapperNativeAOT.Models;

namespace WebApiDapperNativeAOT.Handlers;

public class ToDoHandler
{
    [DapperAot]
    public static Todo GetTodoById(string connectionString, int todoId)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        var response = connection.QueryFirst<Todo>("select Id, Title, DueBy, IsComplete from dbo.Todos where Id=@todoId", new { todoId });
        return response;
    }
}
