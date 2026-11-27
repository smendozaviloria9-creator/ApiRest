using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateProjectDto(
    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [MaxLength(150)]
    string Name,

    [MaxLength(1000)]
    string? Description,

    [Required]
    DateTime StartDate,

    [Required]
    DateTime EndDate
);

public record UpdateProjectDto(
    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [MaxLength(150)]
    string Name,

    [MaxLength(1000)]
    string? Description,

    [Required]
    DateTime StartDate,

    [Required]
    DateTime EndDate
);

public record ProjectResponseDto(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    int TotalTasks,
    int CompletedTasks,
    double ProgressPercentage
);