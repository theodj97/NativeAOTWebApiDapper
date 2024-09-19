using Azure.Core;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using WebApiDapperNativeAOT.Handlers.Mappers;
using WebApiDapperNativeAOT.Models.Entities;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Models.Results;

namespace WebApiDapperNativeAOT.Handlers;

public class TodoHandler(string connectionString)
{
    private readonly string connectionString = connectionString;

    [DapperAot]
    public async Task<Result<IEnumerable<TodoResponse>>> SearchAsync(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, bool? isComplete = null, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var query = new StringBuilder("SELECT Id, Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete FROM dbo.Todos");

        List<string> conditions = [];
        List<SqlParameter> parameters = [];

        if (title?.Length > 0)
        {
            conditions.Add("Title IN (@title)");
            parameters.Add(new SqlParameter("@title", string.Join(",", title)));
        }

        if (description?.Length > 0)
        {
            conditions.Add("Description IN (@description)");
            parameters.Add(new SqlParameter("@description", string.Join(",", description)));
        }

        if (createdBy.HasValue)
        {
            conditions.Add("CreatedBy = @createdBy");
            parameters.Add(new SqlParameter("@createdBy", createdBy));
        }

        if (assignedTo?.Length > 0)
        {
            conditions.Add("AssignedTo IN (@assignedTo)");
            parameters.Add(new SqlParameter("@assignedTo", string.Join(",", assignedTo)));
        }

        if (isComplete.HasValue)
        {
            conditions.Add("IsComplete = @isComplete");
            parameters.Add(new SqlParameter("@isComplete", isComplete));
        }

        if (conditions.Count > 0)
            query.Append(" WHERE ").Append(string.Join(" AND ", conditions));

        using var command = new SqlCommand(query.ToString(), connection);
        command.Parameters.AddRange([.. parameters]);

        var results = await ExecuteReaderAsync(command, cancellationToken);

        if (results.Count == 0)
            return Result<IEnumerable<TodoResponse>>.NoContent();

        var response = TodoMapper.FromEntityToResponse(results);
        return Result<IEnumerable<TodoResponse>>.Success(response);
    }

    [DapperAot]
    public async Task<Result<TodoResponse>> GetByIdAsync(int todoId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var query = "SELECT Id, Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete FROM dbo.Todos WHERE Id = @todoId";
        var parameters = new Dictionary<string, object>
        {
            { "@todoId", todoId }
        };

        var command = new SqlCommand(query, connection);
        foreach (var param in parameters)
            command.Parameters.AddWithValue(param.Key, param.Value);

        var results = await ExecuteReaderAsync(command, cancellationToken);
        if (results.Count != 0)
            return Result<TodoResponse>.Success(TodoMapper.FromEntityToResponse(results.First()));

        return Result<TodoResponse>.Failure(new NotFoundError($"Todo with Id {todoId} not found."));
    }

    [DapperAot]
    public async Task<Result<TodoResponse>> CreateAsync(TodoCreateRequest request, CancellationToken cancellationToken = default)
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
            return Result<TodoResponse>.Created(TodoMapper.FromCreateRequestToResponse(request, newId));
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result<TodoResponse>.Failure(new ConflictError("A Todo with the same title already exists."));
        }
    }

    [DapperAot]
    public async Task<Result<bool>> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken cancellationToken = default)
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

            return Result<bool>.Success(result > 0);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result<bool>.Failure(new ConflictError("A Todo with the same title already exists."));
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
    public async Task<Result<bool>> BulkInsertAsync(IEnumerable<TodoCreateRequest> request, CancellationToken cancellationToken = default)
    {
        // Cambiar este if en un futuro a un validador de requests externo, para que el handler no tenga que preocuparse por la validación
        if (!request.Any())
            return Result<bool>.Failure(new DomainError("Anything to insert"));

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = new StringBuilder();
            var parameters = new List<SqlParameter>();

            query.Append("IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title IN (");
            for (int i = 0; i < request.Count(); i++)
            {
                var todo = request.ElementAt(i);
                query.Append($"@Title{i},");
                parameters.Add(new SqlParameter($"@Title{i}", todo.Title));
            }
            query.Length--; // Eliminar la última coma
            query.Append(")) BEGIN THROW 50000, 'A Todo with the same title already exists.', 1; END ");

            query.Append("INSERT INTO dbo.Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) VALUES ");
            for (int i = 0; i < request.Count(); i++)
            {
                var todo = request.ElementAt(i);
                query.Append($"(@Title{i}, @Description{i}, @CreatedBy{i}, @AssignedTo{i}, @TargetDate{i}, @IsComplete{i}),");
                parameters.Add(new SqlParameter($"@Description{i}", (object?)todo.Description ?? DBNull.Value));
                parameters.Add(new SqlParameter($"@CreatedBy{i}", todo.CreatedBy));
                parameters.Add(new SqlParameter($"@AssignedTo{i}", (object?)todo.AssignedTo ?? DBNull.Value));
                parameters.Add(new SqlParameter($"@TargetDate{i}", todo.TargetDate));
                parameters.Add(new SqlParameter($"@IsComplete{i}", todo.IsComplete));
            }
            query.Length--; // Eliminar la última coma

            using var command = new SqlCommand(query.ToString(), connection, transaction);
            command.Parameters.AddRange([.. parameters]);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(new ConflictError("A Todo with the same title already exists."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [DapperAot]
    public async Task<Result<bool>> BulkUpdateAsync(IEnumerable<TodoBulkUpdateRequest> request, CancellationToken cancellationToken = default)
    {
        // Cambiar este if en un futuro a un validador de requests externo, para que el handler no tenga que preocuparse por la validación
        if (!request.Any())
            return Result<bool>.Failure(new DomainError("Anything to insert"));

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = new StringBuilder();
            var parameters = new List<SqlParameter>();

            query.Append(@"
            CREATE TABLE #TempTodos (
                Id INT,
                Title NVARCHAR(255) NOT NULL,
                Description NVARCHAR(1000),
                CreatedBy INT NOT NULL,
                AssignedTo NVARCHAR(255),
                TargetDate DATETIME,
                IsComplete BIT NOT NULL
            );
            INSERT INTO #TempTodos (Id, Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete)
            VALUES ");

            for (int i = 0; i < request.Count(); i++)
            {
                var todo = request.ElementAt(i);
                query.Append($"(@Id{i}, @Title{i}, @Description{i}, @CreatedBy{i}, @AssignedTo{i}, @TargetDate{i}, @IsComplete{i}),");
                parameters.Add(new SqlParameter($"@Id{i}", todo.Id));
                parameters.Add(new SqlParameter($"@Title{i}", todo.Title));
                parameters.Add(new SqlParameter($"@Description{i}", (object?)todo.Description ?? DBNull.Value));
                parameters.Add(new SqlParameter($"@CreatedBy{i}", todo.CreatedBy));
                parameters.Add(new SqlParameter($"@AssignedTo{i}", (object?)todo.AssignedTo ?? DBNull.Value));
                parameters.Add(new SqlParameter($"@TargetDate{i}", (object?)todo.TargetDate ?? DBNull.Value));
                parameters.Add(new SqlParameter($"@IsComplete{i}", todo.IsComplete));
            }
            query.Length--; // Eliminar la última coma

            query.Append(@";
            IF (SELECT COUNT(*) FROM dbo.Todos WHERE Id IN (SELECT Id FROM #TempTodos)) <> @Count
            BEGIN
                THROW 50000, 'One or more Todo IDs do not exist.', 1;
            END
            IF EXISTS (SELECT 1 FROM dbo.Todos WHERE Title IN (SELECT Title FROM #TempTodos) AND Id NOT IN (SELECT Id FROM #TempTodos))
            BEGIN
                THROW 50001, 'A Todo with the same title already exists.', 1;
            END

            UPDATE t
            SET t.Title = temp.Title,
                t.Description = temp.Description,
                t.CreatedBy = temp.CreatedBy,
                t.AssignedTo = temp.AssignedTo,
                t.TargetDate = temp.TargetDate,
                t.IsComplete = temp.IsComplete
            FROM dbo.Todos t
            INNER JOIN #TempTodos temp ON t.Id = temp.Id;
            ");

            parameters.Add(new SqlParameter("@Count", request.Count()));

            using var command = new SqlCommand(query.ToString(), connection, transaction);
            command.Parameters.AddRange([.. parameters]);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(new NotFoundError("One or more Todo IDs do not exist."));
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(new ConflictError("A Todo with the same title already exists."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS #TempTodos");
        }
    }

    [DapperAot]
    public async Task<Result<bool>> BulkDeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        // Cambiar este if en un futuro a un validador de requests externo, para que el handler no tenga que preocuparse por la validación
        if (!ids.Any())
            return Result<bool>.Failure(new DomainError("Anything to insert"));

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var query = new StringBuilder();
            var parameters = new List<SqlParameter>();

            query.Append("IF (SELECT COUNT(*) FROM dbo.Todos WHERE Id IN (");
            for (int i = 0; i < ids.Count(); i++)
            {
                query.Append($"@Id{i},");
                parameters.Add(new SqlParameter($"@Id{i}", ids.ElementAt(i)));
            }
            query.Length--; // Eliminar la última coma
            query.Append(")) <> @Count BEGIN THROW 50000, 'One or more Todo IDs do not exist.', 1; END ");

            query.Append("DELETE FROM dbo.Todos WHERE Id IN (");
            for (int i = 0; i < ids.Count(); i++)
            {
                query.Append($"@Id{i},");
            }
            query.Length--; // Eliminar la última coma
            query.Append(");");

            parameters.Add(new SqlParameter("@Count", ids.Count()));

            using var command = new SqlCommand(query.ToString(), connection, transaction);
            command.Parameters.AddRange([.. parameters]);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(new NotFoundError("One or more Todo IDs do not exist."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<List<TodoEntity>> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<TodoEntity>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var entity = TodoMapper.MapReaderToTodoEntity(reader);
            results.Add(entity);
        }

        return results;
    }
}
