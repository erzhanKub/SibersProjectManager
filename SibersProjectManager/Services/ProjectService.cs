using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Data;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Constants;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Services
{
    internal sealed class ProjectService(
        AppDbContext appDbContext,
        IUserContextService userContextService,
        ILogger<ProjectService> logger) : IProjectService
    {
        private readonly AppDbContext _context = appDbContext;
        private readonly IUserContextService _userContextService = userContextService;
        private readonly ILogger<ProjectService> _logger = logger;
        private const int _pageSize = 10;

        public async Task<Result<IReadOnlyCollection<Project>>> GetAsync(int page)
        {
            if (page < 1)
                page = 1;

            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<IReadOnlyCollection<Project>>.Failure("User is not authorized");
            }

            var query = _context.Projects
                .Include(p => p.ProjectEmployees)
                .Include(p => p.ProjectManager)
                .AsQueryable();

            if (await _userContextService.IsInRoleAsync(Roles.Employee))
            {
                query = query.Where(p => p.ProjectEmployees.Any(pe => pe.Employee.ApplicationUserId == userId));
            }
            else if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                query = query.Where(p => p.ProjectManager.ApplicationUserId == userId);
            }

            var projects = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (!projects.Any())
            {
                _logger.LogWarning("No projects found on page [{Page}]", page);
                return Result<IReadOnlyCollection<Project>>.Failure($"No projects found on page [{page}]");
            }

            return Result<IReadOnlyCollection<Project>>.Success(projects.AsReadOnly());
        }

        public async Task<Result<Project>> GetByIdAsync(int id)
        {
            if (id < 1)
            {
                _logger.LogWarning("Invalid project ID [{Id}]", id);
                return Result<Project>.Failure($"Invalid project ID [{id}]");
            }

            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<Project>.Failure("User is not authorized");
            }

            var project = await _context.Projects
                .Include(p => p.ProjectEmployees)
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                _logger.LogWarning("Project with ID [{Id}] not found", id);
                return Result<Project>.Failure($"Project with ID [{id}] not found");
            }

            if (await _userContextService.IsInRoleAsync(Roles.Employee))
            {
                if (!project.ProjectEmployees.Any(pe => pe.Employee.ApplicationUserId == userId))
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to project [{ProjectId}]", userId, id);
                    return Result<Project>.Failure("Access denied");
                }
            }
            else if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                if (project.ProjectManager.ApplicationUserId != userId)
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to project [{ProjectId}]", userId, id);
                    return Result<Project>.Failure("Access denied");
                }
            }

            return Result<Project>.Success(project);
        }

        public async Task<Result<int>> CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Project with ID [{Id}] created", project.Id);
                return Result<int>.Success(project.Id);
            }
            else
            {
                _logger.LogError("Failed to create project");
                return Result<int>.Failure("Failed to create project");
            }
        }

        public async Task<Result<bool>> UpdateAsync(Project project)
        {
            var exists = await _context.Projects.AnyAsync(p => p.Id == project.Id);
            if (!exists)
            {
                _logger.LogWarning("Project with ID [{Id}] not found", project.Id);
                return Result<bool>.Failure($"Project with ID [{project.Id}] not found");
            }

            _context.Entry(project).State = EntityState.Modified;
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Project with ID [{Id}] updated", project.Id);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to update project with ID [{Id}]", project.Id);
                return Result<bool>.Failure($"Failed to update project with ID [{project.Id}]");
            }
        }

        public async Task<Result<int>> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project is null)
            {
                _logger.LogWarning("Project with ID [{Id}] not found", id);
                return Result<int>.Failure($"Project with ID [{id}] not found");
            }

            _context.Projects.Remove(project);
            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Project with ID [{Id}] deleted", id);
                return Result<int>.Success(id);
            }
            else
            {
                _logger.LogError("Failed to delete project with ID [{Id}]", id);
                return Result<int>.Failure($"Failed to delete project with ID [{id}]");
            }
        }

        public async Task<Result<bool>> AssignEmployeesAsync(int projectId, IReadOnlyCollection<int> employeeIds)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized access attempt");
                return Result<bool>.Failure("User is not authorized");
            }

            var project = await _context.Projects
                .Include(p => p.ProjectEmployees)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
            {
                _logger.LogWarning("Project with ID [{Id}] not found", projectId);
                return Result<bool>.Failure($"Project with ID [{projectId}] not found");
            }

            if (await _userContextService.IsInRoleAsync(Roles.ProjectManager))
            {
                if (project.ProjectManager.ApplicationUserId != userId)
                {
                    _logger.LogWarning("Access denied for user [{UserId}] to assign employees to project [{ProjectId}]", userId, projectId);
                    return Result<bool>.Failure("Access denied");
                }
            }
            else if (!await _userContextService.IsInRoleAsync(Roles.Administrator))
            {
                _logger.LogWarning("Access denied for user [{UserId}] to assign employees to project [{ProjectId}]", userId, projectId);
                return Result<bool>.Failure("Access denied");
            }

            project.ProjectEmployees.Clear();
            foreach (var empId in employeeIds)
            {
                project.ProjectEmployees.Add(new ProjectEmployee
                {
                    ProjectId = projectId,
                    EmployeeId = empId
                });
            }

            if (await _context.SaveChangesAsync() > 0)
            {
                _logger.LogInformation("Employees assigned to project with ID [{Id}]", projectId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to assign employees to project with ID [{Id}]", projectId);
                return Result<bool>.Failure($"Failed to assign employees to project with ID [{projectId}]");
            }
        }

        public async Task<Result<IReadOnlyCollection<Project>>> GetFilteredAsync(
            int page, DateTime? startFrom, DateTime? startTo,
            Priority? priority, SortBy sortBy = SortBy.Id, bool descending = false)
        {
            var query = _context.Projects
                .Include(p => p.ProjectEmployees)
                .Include(p => p.ProjectManager)
                .AsQueryable();

            if (startFrom.HasValue)
                query = query.Where(p => p.StartDate >= startFrom.Value);

            if (startTo.HasValue)
                query = query.Where(p => p.StartDate <= startTo.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            query = sortBy switch
            {
                SortBy.Name => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                SortBy.StartDate => descending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
                SortBy.Priority => descending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
                _ => query.OrderBy(p => p.Id)
            };

            query = query.Skip((page - 1) * _pageSize).Take(_pageSize);

            var projects = await query.ToListAsync();
            if (!projects.Any())
            {
                _logger.LogWarning("No projects found with the specified filters");
                return Result<IReadOnlyCollection<Project>>.Failure("No projects found with the specified filters");
            }

            return Result<IReadOnlyCollection<Project>>.Success(projects.AsReadOnly());
        }
    }
}