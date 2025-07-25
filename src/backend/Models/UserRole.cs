using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Models;

[PrimaryKey(nameof(UserId), nameof(RoleId))]
public class UserRole
{
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [ForeignKey("Role")]
    public Guid RoleId { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}