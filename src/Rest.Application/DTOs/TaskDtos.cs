using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public record CreateTaskDto(
    [Required(ErrorMessage = "El título de la tarea es obligatorio.")]
    [MaxLength(150)]
    string Title,

    [MaxLength(1000)]
    string? Description,

    TaskPriority Priority,

    [Required]
    DateTime DueDate,

    [Required]
    int ProjectId
);

public record UpdateTaskDto(
    [Required(ErrorMessage = "El título de la tarea es obligatorio.")]
    [MaxLength(150)]
    string Title,

    [MaxLength(1000)]
    string? Description,

    TaskItemStatus Status,
    TaskPriority Priority,

    [Required]
    DateTime DueDate
);

public record UpdateTaskStatusDto(
    [Required]
    TaskItemStatus Status
);

public record TaskResponseDto(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime DueDate,
    int ProjectId
);