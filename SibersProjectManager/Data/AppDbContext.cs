using Microsoft.EntityFrameworkCore;
using SibersProjectManager.Models;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Data
{
    internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectEmployee> ProjectEmployees { get; set; }

        private void ProjectCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .ToTable("projects")
                .HasKey(p => p.Id);

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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



            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Author)
                .WithMany(e => e.AuthoredTasks)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Assignee)
                .WithMany(e => e.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}