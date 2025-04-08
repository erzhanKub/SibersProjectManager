using System.Security.Claims;

namespace SibersProjectManager.Interfaces
{
    internal interface IUserContextService
    {
        string GetCurrentUserId();
        Task<bool> IsInRoleAsync(string role);
        ClaimsPrincipal GetCurrentUser();
    }
}