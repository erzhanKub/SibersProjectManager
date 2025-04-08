using System.ComponentModel.DataAnnotations;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Models
{
    internal sealed record Project
    {
        public int Id { get; init; }

        [Required, MaxLength(200)]
        public string Name { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        [Required, MaxLength(200)]
        public string CustomerCompany { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string ContractorCompany { get; init; } = string.Empty;

        public Priority Priority { get; set; }

        public int? ProjectManagerId { get; init; }
        public Employee ProjectManager { get; init; } = new();

        public ICollection<ProjectEmployee> ProjectEmployees { get; init; } = [];
        public ICollection<ProjectTask> ProjectTasks { get; init; } = [];
    }
}