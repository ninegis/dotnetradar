using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Services
{
    public interface IUserService
    {
        Task<LoginResponse?> AuthenticateAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task InitializeDefaultDataAsync();
    }
}

