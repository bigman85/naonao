using System.ComponentModel.DataAnnotations;

namespace HHPortal.Backend.DTOs
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "角色名称长度必须在2-50个字符之间")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "描述长度不能超过200个字符")]
        public string? Description { get; set; }
    }

    public class UpdateRoleDto
    {
        [StringLength(200, ErrorMessage = "描述长度不能超过200个字符")]
        public string? Description { get; set; }
    }

    public class RoleListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoleDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class AssignPermissionsDto
    {
        public List<Guid> PermissionIds { get; set; } = new();
    }
}