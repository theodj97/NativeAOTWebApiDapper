namespace WebApiDapperNativeAOT.Models.Requests.Todo;

public record TodoBulkUpdateRequest(int Id, string Title, string? Description, int CreatedBy, string? AssignedTo = null, DateTime? TargetDate = null, bool IsComplete = false);
