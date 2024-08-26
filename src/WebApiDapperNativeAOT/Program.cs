using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Handlers.ExceptionHandler;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Routes;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

string connectionString = "Server=localhost,1433;Database=Todo;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Information; });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton(connectionString);
builder.Services.AddTransient<TodoHandler>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception is not null)
            await exceptionHandler.TryHandleAsync(context, exception, context.RequestAborted);
    });
});

app.Urls.Add("http://localhost:5170");

app.MapRoutes();

app.Run();
public partial class Program { }

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(TodoCreateRequest))]
[JsonSerializable(typeof(IEnumerable<TodoCreateRequest>))]
[JsonSerializable(typeof(TodoUpdateRequest))]
[JsonSerializable(typeof(IEnumerable<TodoBulkUpdateRequest>))]
[JsonSerializable(typeof(IEnumerable<TodoResponse>))]
[JsonSerializable(typeof(IEnumerable<int>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }