using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Data;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;

namespace SibersProjectManager.Services
{
    internal sealed class EmployeeService(
        AppDbContext appDbContext,
        ILogger<EmployeeService> logger) : IEmployeeService
    {
        private readonly AppDbContext _context = appDbContext;
        private readonly ILogger<EmployeeService> _logger = logger;

        private const int _pageSize = 10;

        public async Task<Result<IReadOnlyCollection<Employee>>> GetAsync(int page)
        {
            if (page < 1)
                page = 1;

            var employees = await _context.Employees
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (!employees.Any())
            {
                _logger.LogWarning("No employees found on page [{Page}]", page);
                return Result<IReadOnlyCollection<Employee>>.Failure($"No employees found on page [{page}]");
            }

            return Result<IReadOnlyCollection<Employee>>.Success(employees.AsReadOnly());
        }

        public async Task<Result<Employee>> GetByIdAsync(int id)
        {
            if (id < 1)
                return Result<Employee>.Failure($"Invalid employee ID [{id}]");

            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                _logger.LogWarning("Employee with ID [{Id}] not found", id);
                return Result<Employee>.Failure($"Employee with ID [{id}] not found");
            }

            return Result<Employee>.Success(employee);
        }

        public async Task<Result<int>> CreateAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            if (await _context.SaveChangesAsync() >= 0)
            {
                _logger.LogInformation("Employee with ID [{Id}] created", employee.Id);
                return Result<int>.Success(employee.Id);
            }
            else
            {
                _logger.LogError("Failed to create employee with ID [{Id}]", employee.Id);
                return Result<int>.Failure($"Failed to create employee with ID [{employee.Id}]");
            }
        }

        public async Task<Result<int>> UpdateAsync(Employee employee)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Id == employee.Id);
            if (!exists)
            {
                _logger.LogWarning("Employee with ID [{Id}] not found", employee.Id);
                return Result<int>.Failure($"Employee with ID [{employee.Id}] not found");
            }

            _context.Entry(employee).State = EntityState.Modified;
            if (await _context.SaveChangesAsync() >= 0)
            {
                _logger.LogInformation("Employee with ID [{Id}] updated", employee.Id);
                return Result<int>.Success(employee.Id);
            }
            else
            {
                _logger.LogError("Failed to update employee with ID [{Id}]", employee.Id);
                return Result<int>.Failure($"Failed to update employee with ID [{employee.Id}]");
            }
        }

        public async Task<Result<int>> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                _logger.LogWarning("Employee with ID [{Id}] not found", id);
                return Result<int>.Failure($"Employee with ID [{id}] not found");
            }

            _context.Employees.Remove(employee);
            if (await _context.SaveChangesAsync() >= 0)
            {
                _logger.LogInformation("Employee with ID [{Id}] deleted", id);
                return Result<int>.Success(id);
            }
            else
            {
                _logger.LogError("Failed to delete employee with ID [{Id}]", id);
                return Result<int>.Failure($"Failed to delete employee with ID [{id}]");
            }
        }
    }
}