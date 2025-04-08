using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SibersProjectManager.Interfaces;
using SibersProjectManager.Models;

namespace SibersProjectManager.Services
{
    internal sealed class UserContextService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager) : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public string GetCurrentUserId()
            => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        public ClaimsPrincipal GetCurrentUser()
            => _httpContextAccessor.HttpContext?.User!;

        public async Task<bool> IsInRoleAsync(string role)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            return user is not null && await _userManager.IsInRoleAsync(user, role);
        }
    }
}