using System.ComponentModel.DataAnnotations;

namespace HHPortal.Backend.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = new();
    }

    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "刷新令牌不能为空")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class TokenValidationDto
    {
        public string Token { get; set; } = string.Empty;
    }
}