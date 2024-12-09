using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApiDapperNativeAOT.Models.Results;

namespace WebApiDapperNativeAOT.Handlers.ExceptionHandler;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is null)
            return false;

        ResultModel resultModel = new()
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "Server Error",
            Detail = "An error occurred while processing your request."
        };

        logger.LogError("Error: {exception}", exception);
        Debug.WriteLine($"Error: {exception}");

        httpContext.Response.StatusCode = resultModel.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(resultModel, cancellationToken); // Esta advertencia no es importante ya que se ha añadido la clase ProblemDetails en AppJsonSerializerContext

        return true;
    }
}