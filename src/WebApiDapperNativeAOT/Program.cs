using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using WebApiDapperNativeAOT.Handlers.ExceptionHandler;
using WebApiDapperNativeAOT.Handlers.Todo;
using WebApiDapperNativeAOT.Models.Configuration;
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

if (environment == AppConfiguration.DEVELOPMENT_ENVIRONMENT)
    builder.Configuration.AddUserSecrets<Program>();
else if (environment == AppConfiguration.INTEGRATION_ENVIRONMENT
    || environment == AppConfiguration.STAGING_ENVIRONMENT
    || environment == AppConfiguration.PRODUCTION_ENVIRONMENT)
    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//Handlers
builder.Services.AddTransient<TodoHandler>();

builder.Services.AddTransient(_ => new SqlConnection(builder.Configuration.GetConnectionString(AppConfiguration.CONNECTION_STRING_NAME) ?? throw new Exception("ConnectionString was not found")));

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