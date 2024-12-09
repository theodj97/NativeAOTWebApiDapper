﻿namespace WebApiDapperNativeAOT.Models.Results;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public T Value { get; }
    public bool IsNoContent { get; } = false;
    public bool IsCreated { get; } = false;

    protected Result(T value, bool isSuccess, Error? error, bool isNoContent = false, bool isCreated = false)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException();
        if (!isSuccess && error is null)
            throw new InvalidOperationException();

        // Posibles códigos de respuesta que también son correctos
        var countPossibleResponses = new[] { isNoContent, isCreated }.Count(x => x);
        if (countPossibleResponses > 1)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
        IsNoContent = isNoContent;
        IsCreated = isCreated;
    }

    public static Result<T> Success(T value) => new(value, true, null);
    public static Result<T> Failure(Error error) => new(default, false, error);
    public static Result<T> NoContent() => new(default, true, null, isNoContent: true);
    public static Result<T> Created(T value) => new(value, true, null, isCreated: true);

    public IResult ToResult()
    {
        if (IsSuccess)
        {
            if (IsNoContent)
                return Microsoft.AspNetCore.Http.Results.NoContent();
            if (IsCreated)
                return Microsoft.AspNetCore.Http.Results.Created(string.Empty, Value);

            return Microsoft.AspNetCore.Http.Results.Ok(Value);
        }

        if (Error is null)
            throw new InvalidOperationException();

        return Error switch
        {
            BadRequestError => Microsoft.AspNetCore.Http.Results.BadRequest(new ResultModel() { StatusCode = StatusCodes.Status400BadRequest, Title = Error.Title ?? "Bad Request", Type = "Bad Request", Detail = Error.Description ?? string.Empty }),
            DomainError => Microsoft.AspNetCore.Http.Results.BadRequest(new ResultModel() { StatusCode = StatusCodes.Status400BadRequest, Title = Error.Title ?? "Bad Request", Type = "Bad Request", Detail = Error.Description ?? string.Empty }),
            NotFoundError => Microsoft.AspNetCore.Http.Results.NotFound(new ResultModel() { StatusCode = StatusCodes.Status404NotFound, Title = Error.Title ?? "Not Found", Type = "Not Found", Detail = Error.Description ?? string.Empty }),
            ConflictError => Microsoft.AspNetCore.Http.Results.Conflict(new ResultModel() { StatusCode = StatusCodes.Status409Conflict, Title = Error.Title ?? "Conflict", Type = "Conflict", Detail = Error.Description ?? string.Empty }),
            UnauthorizedError => Microsoft.AspNetCore.Http.Results.Unauthorized(),
            ForbiddenError => Microsoft.AspNetCore.Http.Results.Forbid(),
            _ => throw new Exception("Unknown error type")
        };
    }
}
