using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;

namespace SibersProjectManager.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/employees")]
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
        public async Task<IActionResult> GetAll([FromQuery] int page = 1)
        {
            var result = await _employeeService.GetAsync(page);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _employeeService.GetByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }

        [HttpPost("create")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for Employee: [{Errors}]", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _employeeService.CreateAsync(employee);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("update")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] Employee employee)
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

        [HttpDelete("{id}")]
        [MapToApiVersion(1)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.ErrorMessage);
        }
    }
}