using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;

namespace SibersProjectManager.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/employees")]
    [Authorize(Roles = "Administrator")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    internal sealed class EmployeesController(
        IEmployeeService employeeService,
        ILogger<EmployeesController> logger) : ControllerBase
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly ILogger<EmployeesController> _logger = logger;

        [HttpGet]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(IReadOnlyCollection<Employee>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 1)
        {
            var result = await _employeeService.GetAsync(page);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            var result = await _employeeService.GetByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpPost("create")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAsync([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Employee: [{Errors}]", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _employeeService.CreateAsync(employee);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("update")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAsync([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Employee: [{Errors}]", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _employeeService.UpdateAsync(employee);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id:int}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }
    }
}