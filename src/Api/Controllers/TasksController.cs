using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar y filtrar tareas", Description = "Obtiene tareas filtradas opcionalmente por estado y prioridad.")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetFiltered(
        [FromQuery] TaskItemStatus? status,
        [FromQuery] TaskPriority? priority)
    {
        var tasks = await _taskService.GetFilteredAsync(status, priority);
        return Ok(tasks);
    }

    [HttpGet("overdue")]
    [SwaggerOperation(Summary = "Consultar tareas vencidas no completadas", Description = "Obtiene todas las tareas cuya fecha límite ya pasó y no están en estado Completada.")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetOverdueUncompleted()
    {
        var tasks = await _taskService.GetOverdueUncompletedAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Obtener una tarea por ID")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        return Ok(task);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crear una nueva tarea")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> Create([FromBody] CreateTaskDto dto)
    {
        var created = await _taskService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Actualizar una tarea existente")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var updated = await _taskService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    [SwaggerOperation(Summary = "Cambiar el estado de una tarea")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        var updated = await _taskService.UpdateStatusAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Eliminar una tarea")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }
}