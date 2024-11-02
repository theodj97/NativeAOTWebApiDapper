using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

if (environment == "DEVELOPMENT")
    builder.Configuration.AddUserSecrets<Program>();
else if (environment == "INTEGRATION" || environment == "STAGING" || environment == "PRODUCTION")
    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Information; });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddTransient<TodoHandler>();

builder.Services.AddTransient(_ => new SqlConnection(builder.Configuration.GetConnectionString("TodoSQL") ?? throw new Exception("ConnectionString was not found")));

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