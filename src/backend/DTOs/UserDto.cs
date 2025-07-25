using System.ComponentModel.DataAnnotations;

namespace HHPortal.Backend.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string Password { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string? FullName { get; set; }

        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserDto
    {
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string? FullName { get; set; }

        public List<string>? Roles { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "当前密码不能为空")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserListDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }

    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}