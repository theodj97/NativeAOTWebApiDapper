using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WebApiDapperNativeAOT.Handlers;
using WebApiDapperNativeAOT.Models.Configuration;
using WebApiDapperNativeAOT.Models.Requests.Todo;
using WebApiDapperNativeAOT.Models.Responses;
using WebApiDapperNativeAOT.Routes;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);

if (appSettings.ConnectionStrings is null || appSettings.ConnectionStrings.TodoDB.IsNullOrEmpty())
    throw new Exception("Error reading appsettings file.");

builder.Services.AddSingleton(appSettings);
builder.Services.AddTransient<TodoHandler>();

var app = builder.Build();

app.MapRoutes();

app.Run();
public partial class Program { }

[JsonSerializable(typeof(TodoCreateRequest))]
[JsonSerializable(typeof(TodoUpdateRequest))]
[JsonSerializable(typeof(IEnumerable<TodoResponse>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }