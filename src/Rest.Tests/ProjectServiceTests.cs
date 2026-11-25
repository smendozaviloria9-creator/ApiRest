using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;

namespace Rest.Tests;

[TestFixture]
public class ProjectServiceTests
{
    private Mock<IProjectRepository> _projectRepoMock = null!;
    private Mock<ITaskRepository> _taskRepoMock = null!;
    private ProjectService _service = null!;

    [SetUp]
    public void Setup()
    {
        _projectRepoMock = new Mock<IProjectRepository>();
        _taskRepoMock = new Mock<ITaskRepository>();
        _service = new ProjectService(_projectRepoMock.Object, _taskRepoMock.Object);
    }

    [Test]
    public async Task GetAllAsync_CalculatesProgressCorrectly()
    {
        var projects = new List<Project>
        {
            new()
            {
                Id = 1,
                Name = "Proyecto A",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                Tasks = new List<TaskItem>
                {
                    new() { Id = 1, Status = TaskItemStatus.Completada },
                    new() { Id = 2, Status = TaskItemStatus.Pendiente },
                    new() { Id = 3, Status = TaskItemStatus.EnProgreso },
                    new() { Id = 4, Status = TaskItemStatus.Completada }
                }
            },
            new()
            {
                Id = 2,
                Name = "Proyecto Sin Tareas",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                Tasks = new List<TaskItem>()
            }
        };

        _projectRepoMock.Setup(r => r.GetAllWithTasksAsync()).ReturnsAsync(projects);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].TotalTasks, Is.EqualTo(4));
        Assert.That(result[0].CompletedTasks, Is.EqualTo(2));
        Assert.That(result[0].ProgressPercentage, Is.EqualTo(50.0));
        Assert.That(result[1].ProgressPercentage, Is.EqualTo(0.0));
    }

    [Test]
    public void GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _projectRepoMock.Setup(r => r.GetByIdWithTasksAsync(99)).ReturnsAsync((Project?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Test]
    public async Task CreateAsync_WithValidDates_ReturnsCreatedProject()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var dto = new CreateProjectDto("  Nuevo Proyecto  ", "  Descripción  ", start, end);

        _projectRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Project>()))
            .ReturnsAsync((Project p) =>
            {
                p.Id = 10;
                return p;
            });

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Name, Is.EqualTo("Nuevo Proyecto"));
        Assert.That(result.Description, Is.EqualTo("Descripción"));
        Assert.That(result.ProgressPercentage, Is.EqualTo(0));
    }

    [Test]
    public void CreateAsync_WhenEndDateBeforeStartDate_ThrowsBusinessRuleException()
    {
        var start = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var dto = new CreateProjectDto("Proyecto Inválido", null, start, end);

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(dto));
    }

    [Test]
    public void DeleteAsync_WhenProjectHasPendingTasks_ThrowsBusinessRuleException()
    {
        var project = new Project { Id = 1, Name = "Proyecto Activo" };
        _projectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        _taskRepoMock.Setup(r => r.HasPendingTasksByProjectIdAsync(1)).ReturnsAsync(true);

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeleteAsync(1));
        _projectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_WhenProjectHasNoPendingTasks_DeletesProject()
    {
        var project = new Project { Id = 1, Name = "Proyecto Completado" };
        _projectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        _taskRepoMock.Setup(r => r.HasPendingTasksByProjectIdAsync(1)).ReturnsAsync(false);

        await _service.DeleteAsync(1);

        _projectRepoMock.Verify(r => r.DeleteAsync(project), Times.Once);
    }
}
