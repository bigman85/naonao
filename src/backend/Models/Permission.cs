using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HHPortal.Backend.Models;

public class Permission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "varchar")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "varchar")]
    public string Code { get; set; } = string.Empty;

    [Required]
    public ResourceType ResourceType { get; set; }

    [MaxLength(255)]
    [Column(TypeName = "varchar")]
    public string ResourcePath { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    [ForeignKey("ParentId")]
    public Permission? Parent { get; set; }

    public ICollection<Permission> Children { get; set; } = new List<Permission>();

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public enum ResourceType
{
    Menu = 1,
    Button = 2,
    Api = 3,
    File = 4
}