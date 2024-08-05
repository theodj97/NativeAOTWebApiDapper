using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models;
using WebApiDapperNativeAOT.Models.Configuration;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);

if (appSettings.ConnectionStrings is null || appSettings.ConnectionStrings.TodoDB.IsNullOrEmpty())
    throw new Exception("Error reading appsettings file.");

ToDoHandler handler = new(appSettings);
var todosApi = app.MapGroup("/todos");
//todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) => handler.GetTodoById(id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();
public partial class Program { }

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }