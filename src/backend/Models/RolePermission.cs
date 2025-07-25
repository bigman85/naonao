using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Models;

[PrimaryKey(nameof(RoleId), nameof(PermissionId))]
public class RolePermission
{
    [ForeignKey("Role")]
    public Guid RoleId { get; set; }

    [ForeignKey("Permission")]
    public Guid PermissionId { get; set; }

    [InverseProperty("RolePermissions")]
        public Role? Role { get; set; }
        [InverseProperty("RolePermissions")]
        public Permission? Permission { get; set; }
}