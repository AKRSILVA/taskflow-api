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
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAll()
    {
        var userId = User.GetUserId();

        var projects = await _db.Projects
            .Where(p => p.OwnerId == userId)
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.Tasks.Count))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id)
    {
        var userId = User.GetUserId();

        var project = await _db.Projects
            .Where(p => p.Id == id && p.OwnerId == userId)
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.Tasks.Count))
            .SingleOrDefaultAsync();

        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(ProjectRequest request)
    {
        var userId = User.GetUserId();

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            OwnerId = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var response = new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAtUtc, 0);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProjectRequest request)
    {
        var userId = User.GetUserId();
        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project is null)
        {
            return NotFound();
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project is null)
        {
            return NotFound();
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
