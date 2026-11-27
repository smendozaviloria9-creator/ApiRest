using Api.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Rest.Tests.Api;

[TestFixture]
public class ProjectsControllerTests
{
    private Mock<IProjectService> _projectServiceMock = null!;
    private Mock<ITaskService> _taskServiceMock = null!;
    private ProjectsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _taskServiceMock = new Mock<ITaskService>();
        _controller = new ProjectsController(_projectServiceMock.Object, _taskServiceMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithProjects()
    {
        var projects = new List<ProjectResponseDto>
        {
            new(1, "Proyecto 1", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 2, 1, 50.0)
        };
        _projectServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(projects);

        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(projects));
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new CreateProjectDto("Nuevo", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var created = new ProjectResponseDto(5, "Nuevo", null, dto.StartDate, dto.EndDate, 0, 0, 0);
        _projectServiceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(ProjectsController.GetById)));
        Assert.That(createdResult.Value, Is.EqualTo(created));
    }

    [Test]
    public async Task GetTasksByProjectId_ReturnsOkWithTasks()
    {
        var tasks = new List<TaskResponseDto>
        {
            new(1, "Tarea 1", null, TaskItemStatus.Pendiente, TaskPriority.Alta, DateTime.UtcNow.AddDays(1), 1)
        };
        _taskServiceMock.Setup(s => s.GetByProjectIdAsync(1)).ReturnsAsync(tasks);

        var result = await _controller.GetTasksByProjectId(1);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(tasks));
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(1);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _projectServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
    }
}
