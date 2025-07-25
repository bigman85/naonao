using System.ComponentModel.DataAnnotations;

namespace HHPortal.Backend.DTOs
{
    public class CreatePermissionDto
    {
        [Required(ErrorMessage = "权限名称不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "权限名称长度必须在2-50个字符之间")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "权限代码不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "权限代码长度必须在2-50个字符之间")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "资源类型不能为空")]
        public string ResourceType { get; set; } = string.Empty;

        public string? ResourcePath { get; set; }

        public Guid? ParentId { get; set; }
    }

    public class UpdatePermissionDto
    {
        [StringLength(50, ErrorMessage = "权限名称长度不能超过50个字符")]
        public string? Name { get; set; }

        [StringLength(50, ErrorMessage = "权限代码长度不能超过50个字符")]
        public string? Code { get; set; }

        public string? ResourceType { get; set; }

        public string? ResourcePath { get; set; }

        public Guid? ParentId { get; set; }
    }

    public class PermissionListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string? ResourcePath { get; set; }
        public Guid? ParentId { get; set; }
        public List<PermissionListDto> Children { get; set; } = new();
    }

    public class PermissionDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string? ResourcePath { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}