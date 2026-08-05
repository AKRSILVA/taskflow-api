using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/tasks")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    private async Task<bool> ProjectBelongsToUserAsync(int projectId)
    {
        var userId = User.GetUserId();
        return await _db.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetAll(int projectId)
    {
        if (!await ProjectBelongsToUserAsync(projectId))
        {
            return NotFound();
        }

        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .Select(t => new TaskItemResponse(t.Id, t.Title, t.Description, t.Status, t.DueDateUtc, t.CreatedAtUtc, t.ProjectId))
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItemResponse>> GetById(int projectId, int id)
    {
        if (!await ProjectBelongsToUserAsync(projectId))
        {
            return NotFound();
        }

        var task = await _db.Tasks
            .Where(t => t.Id == id && t.ProjectId == projectId)
            .Select(t => new TaskItemResponse(t.Id, t.Title, t.Description, t.Status, t.DueDateUtc, t.CreatedAtUtc, t.ProjectId))
            .SingleOrDefaultAsync();

        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponse>> Create(int projectId, TaskItemRequest request)
    {
        if (!await ProjectBelongsToUserAsync(projectId))
        {
            return NotFound();
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status,
            DueDateUtc = request.DueDateUtc,
            ProjectId = projectId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        var response = new TaskItemResponse(task.Id, task.Title, task.Description, task.Status, task.DueDateUtc, task.CreatedAtUtc, task.ProjectId);
        return CreatedAtAction(nameof(GetById), new { projectId, id = task.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int projectId, int id, TaskItemRequest request)
    {
        if (!await ProjectBelongsToUserAsync(projectId))
        {
            return NotFound();
        }

        var task = await _db.Tasks.SingleOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);
        if (task is null)
        {
            return NotFound();
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.Status = request.Status;
        task.DueDateUtc = request.DueDateUtc;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        if (!await ProjectBelongsToUserAsync(projectId))
        {
            return NotFound();
        }

        var task = await _db.Tasks.SingleOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);
        if (task is null)
        {
            return NotFound();
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
