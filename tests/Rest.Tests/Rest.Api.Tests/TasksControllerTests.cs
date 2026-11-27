using Api.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Rest.Tests.Api;

[TestFixture]
public class TasksControllerTests
{
    private Mock<ITaskService> _taskServiceMock = null!;
    private TasksController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _controller = new TasksController(_taskServiceMock.Object);
    }

    [Test]
    public async Task GetFiltered_ReturnsOkWithTasks()
    {
        var tasks = new List<TaskResponseDto>
        {
            new(1, "Tarea", null, TaskItemStatus.Pendiente, TaskPriority.Media, DateTime.UtcNow.AddDays(2), 1)
        };
        _taskServiceMock.Setup(s => s.GetFilteredAsync(TaskItemStatus.Pendiente, TaskPriority.Media))
            .ReturnsAsync(tasks);

        var result = await _controller.GetFiltered(TaskItemStatus.Pendiente, TaskPriority.Media);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(tasks));
    }

    [Test]
    public async Task GetOverdueUncompleted_ReturnsOkWithOverdueTasks()
    {
        var tasks = new List<TaskResponseDto>
        {
            new(2, "Vencida", null, TaskItemStatus.EnProgreso, TaskPriority.Alta, DateTime.UtcNow.AddDays(-1), 1)
        };
        _taskServiceMock.Setup(s => s.GetOverdueUncompletedAsync()).ReturnsAsync(tasks);

        var result = await _controller.GetOverdueUncompleted();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(tasks));
    }

    [Test]
    public async Task UpdateStatus_ReturnsOkWithUpdatedTask()
    {
        var dto = new UpdateTaskStatusDto(TaskItemStatus.Completada);
        var updated = new TaskResponseDto(1, "Tarea", null, TaskItemStatus.Completada, TaskPriority.Alta, DateTime.UtcNow, 1);
        _taskServiceMock.Setup(s => s.UpdateStatusAsync(1, dto)).ReturnsAsync(updated);

        var result = await _controller.UpdateStatus(1, dto);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(updated));
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(1);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _taskServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
    }
}
