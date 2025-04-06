using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;
using TaskStatus = SibersProjectManager.Models.Enums.TaskStatus;

namespace SibersProjectManager.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            if (context.Employees.Any())
                return;

            var employees = new List<Employee>
            {
                new() { FirstName = "Ryan", LastName = "Thomas", Patronymic = "Gosling", Email = "Ryan@gmail.com" },
                new() { FirstName = "Pedro", LastName = "Pascal", Patronymic = "Balmaceda", Email = "Pedro@gmail.com" },
                new() { FirstName = "Harry", LastName = "Potter", Patronymic = "Wizard", Email = "Harry@gmail.com" }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();

            var project = new Project
            {
                Name = "Sibers Project Manager",
                CustomerCompany = "Sibers",
                ContractorCompany = "Blue Sky",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Priority = Priority.High,
                ProjectManagerId = employees[0].Id
            };

            context.Projects.Add(project);
            context.SaveChanges();

            context.ProjectEmployees.AddRange(
                new ProjectEmployee { EmployeeId = employees[0].Id, ProjectId = project.Id },
                new ProjectEmployee { EmployeeId = employees[1].Id, ProjectId = project.Id },
                new ProjectEmployee { EmployeeId = employees[2].Id, ProjectId = project.Id }
            );

            var tasks = new List<ProjectTask>
            {
                new()
                {
                    Title = "Frontend development",
                    Comment = "Create UI components",
                    Status = TaskStatus.ToDo,
                    Priority = Priority.Medium,
                    AuthorId = employees[0].Id,
                    AssigneeId = employees[1].Id,
                    ProjectId = project.Id
                },
                new()
                {
                    Title = "Backend development",
                    Comment = "Create API endpoints",
                    Status = TaskStatus.InProgress,
                    Priority = Priority.High,
                    AuthorId = employees[0].Id,
                    AssigneeId = employees[2].Id,
                    ProjectId = project.Id
                }
            };

            context.ProjectTasks.AddRange(tasks);
            context.SaveChanges();
        }
    }
}