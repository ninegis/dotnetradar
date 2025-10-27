using System.Security.Claims;

namespace RadarSystem.WebAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string username, string role);
        ClaimsPrincipal? ValidateToken(string token);
    }
}

