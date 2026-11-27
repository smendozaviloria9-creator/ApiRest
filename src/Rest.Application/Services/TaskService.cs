using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority)
    {
        var tasks = await _taskRepository.GetFilteredAsync(status, priority);
        return tasks.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {projectId}.");

        var tasks = await _taskRepository.GetByProjectIdAsync(project.Id);
        return tasks.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetOverdueUncompletedAsync()
    {
        var tasks = await _taskRepository.GetOverdueUncompletedAsync(DateTime.UtcNow);
        return tasks.Select(MapToResponseDto);
    }

    public async Task<TaskResponseDto> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId)
            ?? throw new NotFoundException($"No existe el proyecto con ID {dto.ProjectId}.");

        if (dto.DueDate.Date < project.StartDate.Date)
            throw new BusinessRuleException("La fecha límite de la tarea no puede ser anterior al inicio del proyecto.");

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status = TaskItemStatus.Pendiente,
            Priority = dto.Priority,
            DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc),
            ProjectId = dto.ProjectId
        };

        var created = await _taskRepository.AddAsync(task);
        return MapToResponseDto(created);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc);

        await _taskRepository.UpdateAsync(task);
        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> UpdateStatusAsync(int id, UpdateTaskStatusDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        task.Status = dto.Status;
        await _taskRepository.UpdateAsync(task);
        return MapToResponseDto(task);
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        await _taskRepository.DeleteAsync(task);
    }

    private static TaskResponseDto MapToResponseDto(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.DueDate,
        task.ProjectId
    );
}