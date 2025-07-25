using HHPortal.Backend.DTOs;
using HHPortal.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HHPortal.Backend.Models;

namespace HHPortal.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<User> _userManager;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, UserManager<User> userManager)
    {
        _authService = authService;
        _logger = logger;
        _userManager = userManager;
    }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (token, refreshToken) = await _authService.LoginAsync(loginDto.Username, loginDto.Password, loginDto.RememberMe);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }

            var user = await _userManager.FindByNameAsync(loginDto.Username);
            var userInfo = new UserInfoDto
            {
                Id = user?.Id ?? Guid.Empty,
                Username = user?.UserName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Roles = user != null ? (await _userManager.GetRolesAsync(user)).ToList() : new List<string>(),
            };

            var response = new LoginResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 假设令牌有效期为1小时
                User = userInfo
            };

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (result == null)
            {
                return Unauthorized(new { message = "刷新令牌无效或已过期" });
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
            return Ok(new { message = "登出成功" });
        }

        [HttpPost("validate")]
        public async Task<ActionResult> Validate([FromBody] TokenValidationDto tokenValidationDto)
        {
            var isValid = await _authService.ValidateTokenAsync(tokenValidationDto.Token);
            return Ok(new { valid = isValid });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await Task.CompletedTask;

            // 这里可以扩展获取当前用户的详细信息
            var userInfo = new UserInfoDto
            {
                Id = Guid.Parse(userId),
                Username = User.Identity?.Name ?? string.Empty,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
                Roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            };

            return Ok(userInfo);
        }
    }
}