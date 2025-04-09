using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Data;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Constants;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Services
{
    internal sealed class TaskService(
        AppDbContext appDbContext,
        ILogger<TaskService> logger,
        IUserContextService userContextService) : ITaskService
    {
        private readonly AppDbContext _context = appDbContext;
        private readonly ILogger<TaskService> _logger = logger;
        private readonly IUserContextService _userContextService = userContextService;
        private const int _pageSize = 10;

        public async Task<Result<IReadOnlyCollection<ProjectTask>>> GetAsync(
            int projectId, int page, TaskStatus? status)
        {
            if (page < 1)
                page = 1;

            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<IReadOnlyCollection<ProjectTask>>.Failure("User is not authorized");
            }

            var query = _context.ProjectTasks
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .AsQueryable();

            query = query.Where(t => t.ProjectId == projectId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status);

            if (await _userContextService.IsInRoleAsync(Roles.Employee))
            {
                if (int.TryParse(userId, out var userIdAsInt))
                {
                    query = query.Where(t => t.AssigneeId == userIdAsInt);
                }
                else
                {
                    _logger.LogWarning("Failed to parse userId [{UserId}] to int", userId);
                    return Result<IReadOnlyCollection<ProjectTask>>.Failure("Invalid user ID");
                }
            }
            else if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                var projects = await _context.Projects
                    .Where(p => p.ProjectManager.ApplicationUserId == userId)
                    .Select(p => p.Id)
                    .ToListAsync();

                query = query.Where(t => projects.Contains(t.ProjectId));
            }

            var tasks = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (!tasks.Any())
            {
                _logger.LogWarning("No tasks found for project [{ProjectId}] on page [{Page}]", projectId, page);
                return Result<IReadOnlyCollection<ProjectTask>>.Success([]);
            }

            return Result<IReadOnlyCollection<ProjectTask>>.Success(tasks.AsReadOnly());
        }

        public async Task<Result<ProjectTask>> GetByIdAsync(int id)
        {
            if (id < 1)
            {
                _logger.LogWarning("Invalid task ID [{Id}]", id);
                return Result<ProjectTask>.Failure($"Invalid task ID [{id}]");
            }

            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<ProjectTask>.Failure("User is not authorized");
            }

            var task = await _context.ProjectTasks
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task is null)
            {
                _logger.LogWarning("Task with ID [{Id}] not found", id);
                return Result<ProjectTask>.Failure($"Task with ID [{id}] not found");
            }

            if (await _userContextService.IsInRoleAsync(Roles.Employee))
            {
                if (task.AssigneeId != int.Parse(userId))
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to task [{TaskId}]", userId, id);
                    return Result<ProjectTask>.Failure("Access denied");
                }
            }
            else if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                var managerProjects = await _context.Projects
                    .Where(p => p.ProjectManager.ApplicationUserId == userId)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (!managerProjects.Contains(task.ProjectId))
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to task [{TaskId}]", userId, id);
                    return Result<ProjectTask>.Failure("Access denied");
                }
            }

            return Result<ProjectTask>.Success(task);
        }

        public async Task<Result<int>> CreateAsync(ProjectTask task)
        {
            _context.ProjectTasks.Add(task);
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Task with ID [{Id}] created", task.Id);
                return Result<int>.Success(task.Id);
            }
            else
            {
                _logger.LogError("Failed to create task with ID [{Id}]", task.Id);
                return Result<int>.Failure($"Failed to create task with ID [{task.Id}]");
            }
        }

        public async Task<Result<bool>> UpdateAsync(ProjectTask task)
        {
            var exists = await _context.ProjectTasks.AnyAsync(t => t.Id == task.Id);
            if (!exists)
            {
                _logger.LogWarning("Task with ID [{Id}] not found", task.Id);
                return Result<bool>.Failure($"Task with ID [{task.Id}] not found");
            }

            _context.Entry(task).State = EntityState.Modified;
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Task with ID [{Id}] updated", task.Id);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to update task with ID [{Id}]", task.Id);
                return Result<bool>.Failure($"Failed to update task with ID [{task.Id}]");
            }
        }

        public async Task<Result<int>> DeleteAsync(int id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task is null)
            {
                _logger.LogWarning("Task with ID [{Id}] not found", id);
                return Result<int>.Failure($"Task with ID [{id}] not found");
            }

            _context.ProjectTasks.Remove(task);
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Task with ID [{Id}] deleted", id);
                return Result<int>.Success(id);
            }
            else
            {
                _logger.LogError("Failed to delete task with ID [{Id}]", id);
                return Result<int>.Failure($"Failed to delete task with ID [{id}]");
            }
        }

        public async Task<Result<bool>> AssignExecutorAsync(int taskId, int executorId)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<bool>.Failure("User is not authorized");
            }

            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task is null)
            {
                _logger.LogWarning("Task with ID [{TaskId}] not found", taskId);
                return Result<bool>.Failure($"Task with ID [{taskId}] not found");
            }

            if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                var managerProjects = await _context.Projects
                    .Where(p => p.ProjectManager.ApplicationUserId == userId)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (!managerProjects.Contains(task.ProjectId))
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to assign executor for task [{TaskId}]", userId, taskId);
                    return Result<bool>.Failure("You can only assign executors for tasks in your projects");
                }
            }

            var employee = await _context.Employees.FindAsync(executorId);
            if (employee is null)
            {
                _logger.LogWarning("Employee with ID [{EmployeeId}] not found", executorId);
                return Result<bool>.Failure($"Employee with ID [{executorId}] not found");
            }

            task.AssigneeId = executorId;
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Executor with ID [{EmployeeId}] assigned to task with ID [{TaskId}]", executorId, taskId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to assign executor with ID [{EmployeeId}] to task with ID [{TaskId}]", executorId, taskId);
                return Result<bool>.Failure($"Failed to assign executor with ID [{executorId}] to task with ID [{taskId}]");
            }
        }

        public async Task<Result<bool>> ChangeStatusAsync(int taskId, TaskStatus newStatus)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<bool>.Failure("User is not authorized");
            }

            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task is null)
            {
                _logger.LogWarning("Task with ID [{TaskId}] not found", taskId);
                return Result<bool>.Failure($"Task with ID [{taskId}] not found");
            }

            if (await _userContextService.IsInRoleAsync(Roles.Employee))
            {
                if (task.AssigneeId != int.Parse(userId))
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to change task [{TaskId}] status", userId, taskId);
                    return Result<bool>.Failure("You can only change the status of your own tasks");
                }
            }

            task.Status = newStatus;
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Status of task with ID [{TaskId}] changed to [{Status}]", taskId, newStatus);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to change status of task with ID [{TaskId}] to [{Status}]", taskId, newStatus);
                return Result<bool>.Failure($"Failed to change status of task with ID [{taskId}] to [{newStatus}]");
            }
        }

        public async Task<Result<IReadOnlyCollection<ProjectTask>>> GetFilteredAsync(
            int page, TaskStatus? status = null, int? projectId = null,
            Priority? priority = Priority.Low, SortBy? sortBy = SortBy.Id, bool descending = false)
        {
            var query = _context.ProjectTasks
                .Include(t => t.Author)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority);

            query = sortBy switch
            {
                SortBy.Name => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                SortBy.Status => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                SortBy.Priority => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                _ => query.OrderBy(t => t.Id)
            };

            query = query.Skip((page - 1) * _pageSize).Take(_pageSize);

            var tasks = await query.AsNoTracking().ToListAsync();
            if (!tasks.Any())
            {
                _logger.LogWarning("No tasks found on page [{Page}]", page);
                return Result<IReadOnlyCollection<ProjectTask>>.Failure($"No tasks found on page [{page}]");
            }

            return Result<IReadOnlyCollection<ProjectTask>>.Success(tasks.AsReadOnly());
        }
    }
}