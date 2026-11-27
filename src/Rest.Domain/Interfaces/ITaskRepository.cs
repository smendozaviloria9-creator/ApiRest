using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetFilteredAsync(TaskItemStatus? status, TaskPriority? priority);
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<TaskItem>> GetOverdueUncompletedAsync(DateTime utcNow);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<bool> HasPendingTasksByProjectIdAsync(int projectId);
    Task<TaskItem> AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}