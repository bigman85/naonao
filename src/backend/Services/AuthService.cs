using System.Security.Claims;
using HHPortal.Backend.Data;
using HHPortal.Backend.DTOs;
using HHPortal.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<(string Token, string RefreshToken)> LoginAsync(string username, string password, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("登录失败：用户 {Username} 不存在", username);
                    return (string.Empty, string.Empty);
                }
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("登录失败：用户 {Username} 密码错误", username);
                return (string.Empty, string.Empty);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(claims);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {Username} 登录成功", username);

            return (accessToken, refreshToken);
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (refreshTokenEntity == null || refreshTokenEntity.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("刷新令牌无效或已过期");
                return string.Empty;
            }

            refreshTokenEntity.IsRevoked = true;
            _context.RefreshTokens.Update(refreshTokenEntity);

            var roles = await _userManager.GetRolesAsync(refreshTokenEntity.User);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, refreshTokenEntity.User.Id.ToString()),
                new Claim(ClaimTypes.Name, refreshTokenEntity.User.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, refreshTokenEntity.User.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var newAccessToken = _jwtTokenService.GenerateAccessToken(claims);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = refreshTokenEntity.User.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return newAccessToken;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (refreshTokenEntity == null)
            {
                return;
            }

            refreshTokenEntity.IsRevoked = true;
            _context.RefreshTokens.Update(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户登出成功");
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var principal = _jwtTokenService.ValidateToken(token);
            if (principal == null)
            {
                return false;
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            return user != null;
        }
    }
}