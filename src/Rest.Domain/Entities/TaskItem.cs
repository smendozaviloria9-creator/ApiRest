using Domain.Enums;

namespace Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pendiente;
    public TaskPriority Priority { get; set; } = TaskPriority.Media;
    public DateTime DueDate { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}