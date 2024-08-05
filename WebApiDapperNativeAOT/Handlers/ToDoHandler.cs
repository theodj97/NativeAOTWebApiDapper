using Dapper;
using Microsoft.Data.SqlClient;
using WebApiDapperNativeAOT.Models;
using WebApiDapperNativeAOT.Models.Configuration;

namespace WebApiDapperNativeAOT.Handlers;

public class ToDoHandler(AppSettings appSettings)
{
    private readonly AppSettings appSettings = appSettings;

    //[DapperAot]
    //public static Todo[] Search(string[]? title = null, string[]? description = null, int[]? createdBy = null, int[]? AssignedTo = null, bool? isComplete = null)
    //{

    //}

    [DapperAot]
    public Todo GetTodoById(int todoId)
    {
        using var connection = new SqlConnection(appSettings.ConnectionStrings.TodoDB);
        connection.Open();
        var response = connection.QueryFirst<Todo>("select Id, Title, Description, CreatedBy, AssignedTo,TargetDate, IsComplete from dbo.Todos where Id=@todoId", new { todoId });
        return response;
    }
}
