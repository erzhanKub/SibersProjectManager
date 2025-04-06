using SibersProjectManager.Models;

namespace SibersProjectManager.Interfaces
{
    internal interface IEmployeeService
    {
        Task<Result<int>> CreateAsync(Employee employee);
        Task<Result<int>> DeleteAsync(int id);
        Task<Result<IReadOnlyCollection<Employee>>> GetAsync(int page);
        Task<Result<Employee>> GetByIdAsync(int id);
        Task<Result<int>> UpdateAsync(Employee employee);
    }
}