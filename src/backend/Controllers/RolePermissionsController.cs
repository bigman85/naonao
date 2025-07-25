using HHPortal.Backend.Data;
using HHPortal.Backend.DTOs;
using HHPortal.Backend.Models;
using Permission = HHPortal.Backend.Models.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolePermissionsController> _logger;

        public RolePermissionsController(ApplicationDbContext context, ILogger<RolePermissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("roles/{roleId}/permissions")]
        public async Task<ActionResult<List<PermissionListDto>>> GetRolePermissions(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .ThenInclude(p => p!.Children)
                .ToListAsync();

            var permissions = rolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission!)
                .Where(p => p.ParentId == null)
                .ToList<Permission>();

            var permissionDtos = BuildPermissionTree(permissions);
            return Ok(permissionDtos);
        }

        [HttpPost("roles/{roleId}/permissions")]
        public async Task<ActionResult> AssignPermissionsToRole(Guid roleId, [FromBody] AssignPermissionsDto assignPermissionsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            // 验证权限是否存在
            var validPermissionIds = new HashSet<Guid>(assignPermissionsDto.PermissionIds);
            var existingPermissions = await _context.Permissions
                .Where(p => validPermissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (existingPermissions.Count != (assignPermissionsDto.PermissionIds?.Count ?? 0))
            {
                return BadRequest(new { message = "存在无效的权限ID" });
            }

            // 删除该角色现有的所有权限
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existingRolePermissions);

            // 添加新的权限关联
            var newRolePermissions = assignPermissionsDto.PermissionIds?.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            }) ?? Enumerable.Empty<RolePermission>();

            _context.RolePermissions.AddRange(newRolePermissions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("角色 {RoleName} 的权限已更新", role.Name);
            return Ok(new { message = "权限分配成功" });
        }

        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                return NotFound(new { message = "权限不存在" });
            }

            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
                return NotFound(new { message = "该角色没有此权限" });
            }

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("已从角色 {RoleName} 移除权限 {PermissionName}", role.Name, permission.Name);
            return Ok(new { message = "权限移除成功" });
        }

        [HttpGet("users/{userId}/permissions")]
        public async Task<ActionResult<List<string>>> GetUserPermissions(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var permissions = await _context.RolePermissions
                .Where(rp => userRoles.Contains(rp.RoleId) && rp.Permission != null)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Code)
                .Distinct()
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpGet("roles/{roleId}/available-permissions")]
        public async Task<ActionResult<List<PermissionListDto>>> GetAvailablePermissionsForRole(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var availablePermissions = await _context.Permissions
                .Where(p => !assignedPermissionIds.Contains(p.Id) && p.ParentId == null)
                .Include(p => p!.Children)
                .ToListAsync();

            var permissionDtos = BuildPermissionTree(availablePermissions.Where(p => p != null).Select(p => p!).ToList());
            return Ok(permissionDtos);
        }

        private List<PermissionListDto> BuildPermissionTree(List<Permission> permissions)
        {
            return permissions.Select(permission => new PermissionListDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                ResourceType = permission.ResourceType.ToString(),
                ResourcePath = permission.ResourcePath,
                ParentId = permission.ParentId,
               Children = BuildPermissionTree(permission.Children?.Where(c => c != null).Select(c => c!).ToList() ?? new List<Permission>())
            }).ToList();
        }
    }
}