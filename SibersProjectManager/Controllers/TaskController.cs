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
    [Route("api/v{version:apiVersion}/tasks")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    internal sealed class TaskController(
        ITaskService taskService,
        ILogger<TaskController> logger) : ControllerBase
    {
        private readonly ITaskService _taskService = taskService;
        private readonly ILogger<TaskController> _logger = logger;

        [HttpGet]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(IReadOnlyCollection<ProjectTask>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int projectId,
            [FromQuery] int page = 1,
            [FromQuery] TaskStatus? status = null)
        {
            var result = await _taskService.GetAsync(projectId, page, status);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(ProjectTask), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            var result = await _taskService.GetByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpPost("create")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAsync([FromBody] ProjectTask task)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Task: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _taskService.CreateAsync(task);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("update")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAsync([FromBody] ProjectTask task)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Task: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _taskService.UpdateAsync(task);
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
            var result = await _taskService.DeleteAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpPost("{id:int}/assign-executor")]
        [MapToApiVersion(1)]
        [Authorize(Roles = "Administrator,ProjectManager")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignExecutorAsync([FromRoute] int id, [FromQuery] int executorId)
        {
            var result = await _taskService.AssignExecutorAsync(id, executorId);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id:int}/change-status")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeStatusAsync([FromRoute] int id, [FromQuery] TaskStatus newStatus)
        {
            var result = await _taskService.ChangeStatusAsync(id, newStatus);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("filter")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(IReadOnlyCollection<ProjectTask>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFilteredAsync(
            [FromQuery] int page = 1,
            [FromQuery] TaskStatus? status = null,
            [FromQuery] int? projectId = null,
            [FromQuery] Priority? priority = null,
            [FromQuery] SortBy? sortBy = SortBy.Id,
            [FromQuery] bool descending = false)
        {
            var result = await _taskService.GetFilteredAsync(page, status, projectId, priority, sortBy, descending);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }
    }
}