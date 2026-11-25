using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public ProjectService(IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
    {
        var projects = await _projectRepository.GetAllWithTasksAsync();
        return projects.Select(MapToResponseDto);
    }

    public async Task<ProjectResponseDto> GetByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdWithTasksAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        return MapToResponseDto(project);
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto)
    {
        ValidateDates(dto.StartDate, dto.EndDate);

        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc)
        };

        var created = await _projectRepository.AddAsync(project);
        return MapToResponseDto(created);
    }

    public async Task<ProjectResponseDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        ValidateDates(dto.StartDate, dto.EndDate);

        var project = await _projectRepository.GetByIdWithTasksAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        project.Name = dto.Name.Trim();
        project.Description = dto.Description?.Trim();
        project.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        project.EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);

        await _projectRepository.UpdateAsync(project);
        return MapToResponseDto(project);
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        // Regla de negocio: Impedir eliminación si tiene tareas pendientes (no completadas)
        bool hasPendingTasks = await _taskRepository.HasPendingTasksByProjectIdAsync(id);
        if (hasPendingTasks)
            throw new BusinessRuleException("No se puede eliminar el proyecto porque aún tiene tareas pendientes o en progreso.");

        await _projectRepository.DeleteAsync(project);
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new BusinessRuleException("La fecha de finalización no puede ser anterior a la fecha de inicio.");
    }

    private static ProjectResponseDto MapToResponseDto(Project project)
    {
        int totalTasks = project.Tasks.Count;
        int completedTasks = project.Tasks.Count(t => t.Status == TaskItemStatus.Completada);
        double progress = totalTasks == 0
            ? 0
            : Math.Round((double)completedTasks / totalTasks * 100, 2);

        return new ProjectResponseDto(
            project.Id,
            project.Name,
            project.Description,
            project.StartDate,
            project.EndDate,
            totalTasks,
            completedTasks,
            progress
        );
    }
}