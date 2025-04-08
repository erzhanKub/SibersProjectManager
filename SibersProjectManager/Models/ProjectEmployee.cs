namespace SibersProjectManager.Models
{
    internal sealed record ProjectEmployee
    {
        public int EmployeeId { get; init; }
        public Employee Employee { get; init; } = new();

        public int ProjectId { get; init; }
        public Project Project { get; init; } = new();
    }
}