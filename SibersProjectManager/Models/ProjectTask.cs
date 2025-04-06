using System.ComponentModel.DataAnnotations;
using SibersProjectManager.Models.Enums;

namespace SibersProjectManager.Models
{
    internal sealed record ProjectTask
    {
        public int Id { get; init; }

        [Required, MaxLength(200)]
        public string Title { get; init; } = string.Empty;

        [MaxLength(500)]
        public string Comment { get; init; } = string.Empty;
        public Enums.TaskStatus Status { get; init; }
        public Priority Priority { get; init; } = Priority.Low;

        public int AuthorId { get; init; }
        public Employee Author { get; init; } = new();

        public int? AssigneeId { get; init; }
        public Employee Assignee { get; init; } = new();

        public int ProjectId { get; init; }
        public Project Project { get; init; } = new();
    }
}