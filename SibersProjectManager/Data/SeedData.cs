using Microsoft.AspNetCore.Identity;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Data
{
    internal static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Administrator", "ProjectManager", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (context.Employees.Any())
                return;

            var adminUser = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true
            };
            var managerUser = new ApplicationUser
            {
                UserName = "manager@gmail.com",
                Email = "manager@gmail.com",
                EmailConfirmed = true
            };
            var employeeUser = new ApplicationUser
            {
                UserName = "employee@gmail.com",
                Email = "employee@gmail.com",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }

            if (await userManager.FindByEmailAsync(managerUser.Email) == null)
            {
                await userManager.CreateAsync(managerUser, "Manager@123");
                await userManager.AddToRoleAsync(managerUser, "ProjectManager");
            }

            if (await userManager.FindByEmailAsync(employeeUser.Email) == null)
            {
                await userManager.CreateAsync(employeeUser, "Employee@123");
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }

            var employees = new List<Employee>
            {
                new() { FirstName = "Ryan", LastName = "Thomas", Patronymic = "Gosling", Email = "admin@gmail.com", ApplicationUserId = adminUser.Id },
                new() { FirstName = "Pedro", LastName = "Pascal", Patronymic = "Balmaceda", Email = "manager@gmail.com", ApplicationUserId = managerUser.Id },
                new() { FirstName = "Harry", LastName = "Potter", Patronymic = "Wizard", Email = "employee@gmail.com", ApplicationUserId = employeeUser.Id }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();

            var project = new Project
            {
                Name = "Sibers Project Manager",
                CustomerCompany = "gmail",
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