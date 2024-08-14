using System.Text.Json.Serialization;
using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Routes;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

string connectionString = "Server=localhost,1433;Database=Todo;User Id=sa;Password=Your_password123;TrustServerCertificate=True";

builder.Services.AddSingleton(connectionString);
builder.Services.AddTransient<TodoHandler>();

var app = builder.Build();

app.Urls.Add("http://localhost:5170");

app.MapRoutes();

app.Run();
public partial class Program { }

[JsonSerializable(typeof(TodoCreateRequest))]
[JsonSerializable(typeof(TodoUpdateRequest))]
[JsonSerializable(typeof(IEnumerable<TodoResponse>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }