using System.Text.Json.Serialization;

namespace WebApiDapperNativeAOT.Models;

public record Todo(int Id, string? Title, DateTime? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
