using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var cnnstring = "Server=localhost,1433;Database=Todo;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateTime.UtcNow),
    new(3, "Do the laundry", DateTime.UtcNow.AddDays(1)),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateTime.UtcNow.AddDays(2))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) => ToDoHandler.GetTodoById(cnnstring, id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();
public partial class Program { }
