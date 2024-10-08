﻿namespace WebApiDapperNativeAOT.Models.Entities;

public record TodoEntity(int Id, string Title, string? Description, int CreatedBy, string? AssignedTo = null, DateTime? TargetDate = null, bool IsComplete = false);
