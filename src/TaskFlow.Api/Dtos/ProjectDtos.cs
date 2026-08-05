using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos;

public record ProjectRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    string? Description);

public record ProjectResponse(int Id, string Name, string? Description, DateTime CreatedAtUtc, int TaskCount);
