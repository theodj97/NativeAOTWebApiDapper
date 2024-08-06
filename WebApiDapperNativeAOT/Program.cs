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

ToDoHandler handler = new(appSettings.ConnectionStrings.TodoDB);
var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", (string[]? title, string[]? description, int? createdBy, int[]? assignedTo, bool? isComplete) =>
{
    var todos = handler.Search(title, description, createdBy, assignedTo, isComplete);
    if (todos.Length > 0)
        return Results.Ok(todos);
    return Results.NoContent();
});

todosApi.MapGet("/{id}", (int id) => handler.GetById(id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

todosApi.MapPost("/", (Todo todo) =>
{
    handler.Create(todo);
    return Results.Created();
});

app.Run();
public partial class Program { }

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }