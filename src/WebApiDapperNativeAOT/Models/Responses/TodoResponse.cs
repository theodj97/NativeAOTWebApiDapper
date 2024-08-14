namespace WebApiDapperNativeAOT.Models.Responses;

public record TodoResponse(int Id, string Title, string? Description, int CreatedBy, string? AssignedTo = null, DateTime? TargetDate = null, bool IsComplete = false);
