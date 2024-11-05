namespace WebApiDapperNativeAOT.Models.Requests.Todo;

public record TodoCreateRequest
(
    string Title,
    string? Description,
    int CreatedBy,
    string? AssignedTo = null,
    DateTime? TargetDate = null,
    bool IsComplete = false
);
