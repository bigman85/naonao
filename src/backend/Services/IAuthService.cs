using System.Threading.Tasks;
using HHPortal.Backend.Models;

namespace HHPortal.Backend.Services;

public interface IAuthService
{
    Task<(string Token, string RefreshToken)> LoginAsync(string username, string password, bool rememberMe);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
}