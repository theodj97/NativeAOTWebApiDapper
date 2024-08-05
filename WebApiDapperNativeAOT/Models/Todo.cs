namespace WebApiDapperNativeAOT.Models;

public record Todo(int Id, string Title, string? Description, int CreatedBy, string? AssignedTo = null, DateTime? TargetDate = null, bool IsComplete = false);
