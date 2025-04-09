using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/projects")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    internal sealed class ProjectsController(
        IProjectService projectService,
        ILogger<ProjectsController> logger) : ControllerBase
    {
        private readonly IProjectService _projectService = projectService;
        private readonly ILogger<ProjectsController> _logger = logger;

        [HttpGet]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(IReadOnlyCollection<Project>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 1)
        {
            var result = await _projectService.GetAsync(page);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        public async Task<ActionResult<Project>> GetByIdAsync([FromRoute] int id)
        {
            var result = await _projectService.GetByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpPost("create")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<Project>> CreateAsync(Project project)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Project: [{Errors}]", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _projectService.CreateAsync(project);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("{id:int}/assign-employees")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignEmployeesAsync([FromRoute] int id, IReadOnlyCollection<int> employeeIds)
        {
            if (employeeIds is null || !employeeIds.Any())
            {
                _logger.LogWarning("No employee IDs provided for project ID [{Id}]", id);
                return BadRequest("No employee IDs provided");
            }

            var result = await _projectService.AssignEmployeesAsync(id, employeeIds);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("update")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAsync([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Project: [{Errors}]", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _projectService.UpdateAsync(project);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id:int}")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            var result = await _projectService.DeleteAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpGet("filter")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager,Employee")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Project>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFilteredAsync(
            [FromQuery] int page = 1,
            [FromQuery] DateTime? startFrom = null,
            [FromQuery] DateTime? startTo = null,
            [FromQuery] Priority? priority = null,
            [FromQuery] SortBy sortBy = SortBy.Id,
            [FromQuery] bool descending = false)
        {
            var result = await _projectService.GetFilteredAsync(page, startFrom, startTo, priority,
                sortBy, descending);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }
    }
}