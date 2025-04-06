using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Interfaces
{
    internal interface IProjectService
    {
        Task<Result<bool>> AssignEmployeesAsync(int projectId, IReadOnlyCollection<int> employeeIds);
        Task<Result<int>> CreateAsync(Project project);
        Task<Result<int>> DeleteAsync(int id);
        Task<Result<IReadOnlyCollection<Project>>> GetAsync(int page);
        Task<Result<Project>> GetByIdAsync(int id);
        Task<Result<bool>> UpdateAsync(Project project);
        Task<Result<IReadOnlyCollection<Project>>> GetFilteredAsync(
            int page, DateTime? startFrom, DateTime? startTo,
            Priority? priority, SortBy sortBy = SortBy.Id, bool descending = false);
    }
}