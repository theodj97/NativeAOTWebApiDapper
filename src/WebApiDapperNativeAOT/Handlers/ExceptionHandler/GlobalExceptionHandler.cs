using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebApiDapperNativeAOT.Handlers.ExceptionHandler;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is null)
            return false;

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "Server Error",
            Detail = "An error occurred while processing your request."
        };

        logger.LogError("Error: {exception}", exception);
        Debug.WriteLine($"Error: {exception}");

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails!, cancellationToken); // Esta advertencia no es importante ya que se ha añadido la clase ProblemDetails en AppJsonSerializerContext

        return true;
    }
}