using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Interfaces
{
    internal interface ITaskService
    {
        Task<Result<bool>> AssignExecutorAsync(int taskId, int executorId);
        Task<Result<bool>> ChangeStatusAsync(int taskId, Models.Enums.TaskStatus newStatus);
        Task<Result<int>> CreateAsync(ProjectTask task);
        Task<Result<int>> DeleteAsync(int id);
        Task<Result<IReadOnlyCollection<ProjectTask>>> GetAsync(int projectId, int page, Models.Enums.TaskStatus? status);
        Task<Result<ProjectTask>> GetByIdAsync(int id);
        Task<Result<IReadOnlyCollection<ProjectTask>>> GetFilteredAsync(int page, Models.Enums.TaskStatus? status = null, int? projectId = null, Priority? priority = Priority.Low, SortBy? sortBy = SortBy.Id, bool descending = false);
        Task<Result<bool>> UpdateAsync(ProjectTask task);
    }
}