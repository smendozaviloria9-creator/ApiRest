using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority);
    Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<TaskResponseDto>> GetOverdueUncompletedAsync();
    Task<TaskResponseDto> GetByIdAsync(int id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto);
    Task<TaskResponseDto> UpdateStatusAsync(int id, UpdateTaskStatusDto dto);
    Task DeleteAsync(int id);
}