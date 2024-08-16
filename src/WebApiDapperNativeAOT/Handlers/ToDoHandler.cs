using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using WebApiDapperNativeAOT.Handlers.Mappers;
using WebApiDapperNativeAOT.Models.Entities;
using WebApiDapperNativeAOT.Models.Exceptions;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;

namespace WebApiDapperNativeAOT.Handlers;

public class TodoHandler(string connectionString)
{
    private readonly string connectionString = connectionString;

    [DapperAot]
    public async Task<IEnumerable<TodoResponse>> SearchAsync(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, bool? isComplete = null, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
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
    public async Task<TodoResponse> GetByIdAsync(int todoId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var response = await connection.QueryFirstAsync<TodoEntity>("select Id, Title, Description, CreatedBy, AssignedTo,TargetDate, IsComplete from dbo.Todos where Id=@todoId", new { todoId });
        return TodoMapper.FromEntityToResponse(response);
    }

    [DapperAot]
    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

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

            var newId = await connection.QuerySingleAsync<int>(query, request, transaction);
            await transaction.CommitAsync(cancellationToken);
            return TodoMapper.FromCreateRequestToResponse(request, newId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException($"There is already a Todo with title '{request.Title}', title must be unique! ");
        }
    }

    [DapperAot]
    public async Task<bool> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = @"
            IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title = @Title AND Id <> @Id)
            BEGIN
                THROW 50000, 'A Todo with the same title already exists.', 1;
            END
            ELSE
            BEGIN
                UPDATE dbo.Todos 
                SET Title = @Title, Description = @Description, CreatedBy = @CreatedBy, AssignedTo = @AssignedTo, TargetDate = @TargetDate, IsComplete = @IsComplete 
                WHERE Id = @Id;
            END";

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

            var result = await connection.ExecuteAsync(query, parameters, transaction);
            await transaction.CommitAsync(cancellationToken);
            return result > 0;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException($"There is already a Todo with title '{request.Title}', title must be unique! ");
        }
    }

    [DapperAot]
    public async Task DeleteAsync(int todoId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var query = "DELETE FROM dbo.Todos WHERE Id = @todoId";
        await connection.ExecuteAsync(query, new { todoId });
    }

    [DapperAot]
    public async Task<bool> BulkInsertAsync(IEnumerable<TodoCreateRequest> request, CancellationToken cancellationToken = default)
    {
        if (!request.Any())
            return false;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = new StringBuilder();
            query.Append("IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title IN (");
            foreach (var todo in request)
            {
                query.Append($"'{todo.Title.Replace("'", "''")}',");
            }
            query.Length--; // Eliminar la última coma
            query.Append(")) BEGIN THROW 50000, 'A Todo with the same title already exists.', 1; END ");

            query.Append("INSERT INTO dbo.Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) VALUES ");
            foreach (var todo in request)
            {
                query.Append($"('{todo.Title.Replace("'", "''")}', {(todo.Description is not null ? $"'{todo.Description.Replace("'", "''")}'" : "NULL")}, {todo.CreatedBy}, {todo.AssignedTo}, '{todo.TargetDate:yyyy-MM-dd HH:mm:ss}', {(todo.IsComplete ? 1 : 0)}),");
            }

            query.Length--; // Eliminar la última coma

            await connection.ExecuteAsync(query.ToString(), transaction: transaction);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("An error occurred while inserting the todos.");
        }
    }

    [DapperAot]
    public async Task<bool> BulkUpdateAsync(IEnumerable<TodoBulkUpdateRequest> request, CancellationToken cancellationToken = default)
    {
        if (!request.Any())
            return false;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var ids = string.Join(",", request.Select(r => r.Id));
            var titles = string.Join(",", request.Select(r => $"'{r.Title.Replace("'", "''")}'"));

            var query = new StringBuilder();
            query.Append($@"
            IF (SELECT COUNT(*) FROM dbo.Todos WHERE Id IN ({ids})) <> {request.Count()}
            BEGIN
                THROW 50000, 'One or more Todo IDs do not exist.', 1;
            END
            IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title IN ({titles}) AND Id NOT IN ({ids}))
            BEGIN
                THROW 50001, 'A Todo with the same title already exists.', 1;
            END
            ");

            foreach (var todo in request)
            {
                query.Append($@"
                UPDATE dbo.Todos 
                SET Title = '{todo.Title.Replace("'", "''")}', 
                    Description = {(todo.Description is not null ? $"'{todo.Description.Replace("'", "''")}'" : "NULL")}, 
                    CreatedBy = {todo.CreatedBy}, 
                    AssignedTo = {todo.AssignedTo}, 
                    TargetDate = '{todo.TargetDate:yyyy-MM-dd HH:mm:ss}', 
                    IsComplete = {(todo.IsComplete ? 1 : 0)} 
                WHERE Id = {todo.Id};
            ");
            }

            var test = query.ToString();

            await connection.ExecuteAsync(query.ToString(), transaction: transaction);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new NotFoundException("One or more Todo IDs do not exist.");
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("A Todo with the same title already exists.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("An error occurred while updating the todos.");
        }
    }
}
