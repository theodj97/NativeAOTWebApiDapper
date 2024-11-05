using Dapper;
using Microsoft.Data.SqlClient;
using WebApiDapperNativeAOT.Models.Entities;

namespace WebApiDapperNativeAOT.Testing.SeedWork;

public class TestServerFixtureExtension(string sqlConnectionString)
{
    private readonly string sqlConnectionString = sqlConnectionString ?? throw new Exception($"sqlConnectionString was null on dependency injection in {nameof(TestServerFixtureExtension)}");

    public async Task<TodoEntity> AddDefaultTodo(string title = "title", string description = "description test", int createdBy = 0, string? assignedTo = null, DateTime? targetDate = null, bool isCompleted = false)
    {
        TodoEntity todo = new(default, title, description, createdBy, assignedTo, targetDate, isCompleted);

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = @"
            IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title = @Title)
            BEGIN
                THROW 50000, 'A Todo with the same title already exists.', 1;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) 
                VALUES (@Title, @Description, @CreatedBy, @AssignedTo, @TargetDate, @IsComplete);
                SELECT CAST(SCOPE_IDENTITY() as int);
            END";

            var newId = await connection.QuerySingleAsync<int>(query, todo, transaction);
            await transaction.CommitAsync();
            return new TodoEntity(newId, todo.Title, todo.Description, todo.CreatedBy, todo.AssignedTo, todo.TargetDate, todo.IsComplete);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
