using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SibersProjectManager.Models
{
    internal sealed record Employee
    {
        public int Id { get; init; }

        [Required, MaxLength(200)]
        public string FirstName { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string LastName { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string Patronymic { get; init; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; } = new();

        public ICollection<Project> LeadingProjects { get; init; } = [];
        public ICollection<ProjectEmployee> ProjectEmployees { get; init; } = [];
        public ICollection<ProjectTask> AuthoredTasks { get; init; } = [];
        public ICollection<ProjectTask> AssignedTasks { get; init; } = [];
    }
}