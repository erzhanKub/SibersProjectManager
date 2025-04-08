using Microsoft.AspNetCore.Identity;

namespace SibersProjectManager.Models
{
    internal sealed class ApplicationUser : IdentityUser
    {
        public Employee Employee { get; set; } = new();
    }
}