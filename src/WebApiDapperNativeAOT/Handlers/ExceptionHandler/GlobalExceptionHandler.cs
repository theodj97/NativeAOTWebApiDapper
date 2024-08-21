using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApiDapperNativeAOT.Models.Exceptions;

namespace WebApiDapperNativeAOT.Handlers.ExceptionHandler;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is null)
            return false;

        ProblemDetails problemDetails = exception switch
        {
            BadRequestException => new()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Type = "Bad Request",
                Detail = exception.Message
            },
            DomainException => new()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain error",
                Type = "Bad Request",
                Detail = exception.Message
            },
            NotFoundException => new()
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Type = "Not found",
                Detail = exception.Message
            },
            ConflictException => new()
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Type = "Conflict",
                Detail = exception.Message
            },
            ForbiddenException => new()
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Type = "Forbidden",
                Detail = exception.Message
            },
            _ => new()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Type = "Server Error",
                Detail = "An error occurred while processing your request."
            },
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            logger.LogError("Error: {exception}", exception);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails!, cancellationToken); // Esta advertencia no es importante ya que se ha añadido la clase ProblemDetails en AppJsonSerializerContext

        return true;
    }
}