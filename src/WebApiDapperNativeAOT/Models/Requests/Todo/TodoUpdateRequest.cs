namespace WebApiDapperNativeAOT.Models.Requests.Todo;

public record TodoUpdateRequest
(
    string Title,
    string? Description,
    int CreatedBy,
    string? AssignedTo = null,
    DateTime? TargetDate = null,
    bool IsComplete = false
);
