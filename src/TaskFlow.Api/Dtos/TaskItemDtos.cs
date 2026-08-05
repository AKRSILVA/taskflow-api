using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Dtos;

public record TaskItemRequest(
    [Required, StringLength(200, MinimumLength = 2)] string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDateUtc);

public record TaskItemResponse(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDateUtc,
    DateTime CreatedAtUtc,
    int ProjectId);
