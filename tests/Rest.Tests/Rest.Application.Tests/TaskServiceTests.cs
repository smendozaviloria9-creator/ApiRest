using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;

namespace Rest.Tests.Application;

[TestFixture]
public class TaskServiceTests
{
    private Mock<ITaskRepository> _taskRepoMock = null!;
    private Mock<IProjectRepository> _projectRepoMock = null!;
    private TaskService _service = null!;

    [SetUp]
    public void Setup()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _service = new TaskService(_taskRepoMock.Object, _projectRepoMock.Object);
    }

    [Test]
    public async Task GetFilteredAsync_ReturnsMappedTasks()
    {
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "Tarea 1",
                Status = TaskItemStatus.Pendiente,
                Priority = TaskPriority.Alta,
                DueDate = DateTime.UtcNow.AddDays(2),
                ProjectId = 1
            }
        };

        _taskRepoMock.Setup(r => r.GetFilteredAsync(TaskItemStatus.Pendiente, TaskPriority.Alta))
            .ReturnsAsync(tasks);

        var result = (await _service.GetFilteredAsync(TaskItemStatus.Pendiente, TaskPriority.Alta)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Tarea 1"));
        Assert.That(result[0].Priority, Is.EqualTo(TaskPriority.Alta));
    }

    [Test]
    public void GetByProjectIdAsync_WhenProjectDoesNotExist_ThrowsNotFoundException()
    {
        _projectRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Project?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _service.GetByProjectIdAsync(42));
    }

    [Test]
    public async Task GetOverdueUncompletedAsync_ReturnsOverdueTasks()
    {
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = 5,
                Title = "Tarea Vencida",
                Status = TaskItemStatus.EnProgreso,
                Priority = TaskPriority.Media,
                DueDate = DateTime.UtcNow.AddDays(-1),
                ProjectId = 1
            }
        };

        _taskRepoMock.Setup(r => r.GetOverdueUncompletedAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(tasks);

        var result = (await _service.GetOverdueUncompletedAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(5));
    }

    [Test]
    public void CreateAsync_WhenDueDateBeforeProjectStart_ThrowsBusinessRuleException()
    {
        var project = new Project
        {
            Id = 1,
            Name = "Proyecto",
            StartDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 5, 30, 0, 0, 0, DateTimeKind.Utc)
        };
        _projectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

        var dto = new CreateTaskDto(
            "Tarea temprana",
            null,
            TaskPriority.Baja,
            new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            1);

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_WithValidData_SetsInitialStatusToPendiente()
    {
        var project = new Project
        {
            Id = 1,
            Name = "Proyecto",
            StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 5, 30, 0, 0, 0, DateTimeKind.Utc)
        };
        _projectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) =>
            {
                t.Id = 7;
                return t;
            });

        var dto = new CreateTaskDto(
            "  Implementar Swagger  ",
            "Detalles",
            TaskPriority.Alta,
            new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            1);

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.Title, Is.EqualTo("Implementar Swagger"));
        Assert.That(result.Status, Is.EqualTo(TaskItemStatus.Pendiente));
    }

    [Test]
    public async Task UpdateStatusAsync_UpdatesStatusAndReturnsDto()
    {
        var task = new TaskItem
        {
            Id = 3,
            Title = "Tarea",
            Status = TaskItemStatus.Pendiente,
            Priority = TaskPriority.Media,
            DueDate = DateTime.UtcNow.AddDays(3),
            ProjectId = 1
        };

        _taskRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(task);

        var result = await _service.UpdateStatusAsync(3, new UpdateTaskStatusDto(TaskItemStatus.Completada));

        Assert.That(result.Status, Is.EqualTo(TaskItemStatus.Completada));
        _taskRepoMock.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Test]
    public void DeleteAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepoMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((TaskItem?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(123));
    }
}
