using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Data
{
    internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectEmployee> ProjectEmployees { get; set; }

        private void OnProjectCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .ToTable("projects")
                .HasKey(p => p.Id);

            modelBuilder.Entity<Project>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Project>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name")
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Project>()
                .Property(p => p.StartDate)
                .HasColumnName("start_date");

            modelBuilder.Entity<Project>()
                .Property(p => p.EndDate)
                .HasColumnName("end_date");

            modelBuilder.Entity<Project>()
                .Property(p => p.CustomerCompany)
                .HasMaxLength(200)
                .HasColumnName("customer_company")
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Project>()
                .Property(p => p.ContractorCompany)
                .HasMaxLength(200)
                .HasColumnName("contractor_company")
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Project>()
                .Property(p => p.Priority)
                .HasColumnName("priority")
                .HasDefaultValue(Priority.Low);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectManager)
                .WithMany(e => e.LeadingProjects)
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void OnEmployeeCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .ToTable("employees")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Employee>()
                .Property(e => e.FirstName)
                .IsRequired()
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Employee>()
                .Property(e => e.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Email)
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty)
                .HasColumnName("email");

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .Property(e => e.ApplicationUserId)
                .HasColumnName("application_user_id");

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ApplicationUser)
                .WithOne()
                .HasForeignKey<Employee>(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void OnProjectEmployeeCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectEmployee>()
                .ToTable("project_employee");

            modelBuilder.Entity<ProjectEmployee>()
                .Property(pe => pe.ProjectId)
                .HasColumnName("project_id");

            modelBuilder.Entity<ProjectEmployee>()
                .Property(pe => pe.EmployeeId)
                .HasColumnName("employee_id");

            modelBuilder.Entity<ProjectEmployee>()
                .HasKey(pe => new { pe.ProjectId, pe.EmployeeId });

            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.ProjectId);

            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectEmployees)
                .HasForeignKey(pe => pe.EmployeeId);
        }

        private void OnProjectTaskCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectTask>()
                .ToTable("project_tasks");

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("title");

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Status)
                .HasColumnName("status");

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Priority)
                .HasColumnName("priority")
                .HasDefaultValue(Priority.Low);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Author)
                .WithMany(e => e.AuthoredTasks)
                .HasForeignKey(pt => pt.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Assignee)
                .WithMany(e => e.AssignedTasks)
                .HasForeignKey(pt => pt.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTasks)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnProjectCreating(modelBuilder);
            OnEmployeeCreating(modelBuilder);
            OnProjectEmployeeCreating(modelBuilder);
            OnProjectTaskCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
    }
}