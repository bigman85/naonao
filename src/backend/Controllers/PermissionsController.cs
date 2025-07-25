using HHPortal.Backend.Data;
using HHPortal.Backend.DTOs;
using HHPortal.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(ApplicationDbContext context, ILogger<PermissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<PermissionListDto>>> GetPermissions()
        {
            var permissions = await _context.Permissions
                .Where(p => p.ParentId == null)
                .Include(p => p.Children)
                .ToListAsync();

            var permissionDtos = BuildPermissionTree(permissions);
            return Ok(permissionDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDetailDto>> GetPermission(Guid id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = "权限不存在" });
            }

            var parent = permission.ParentId.HasValue ? 
                await _context.Permissions.FindAsync(permission.ParentId) : null;

            var permissionDto = new PermissionDetailDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                ResourceType = permission.ResourceType.ToString(),
                ResourcePath = permission.ResourcePath,
                ParentId = permission.ParentId,
                ParentName = parent?.Name,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(permissionDto);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionDetailDto>> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 检查权限代码是否已存在
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == createPermissionDto.Code);
            if (existingPermission != null)
            {
                return BadRequest(new { message = "权限代码已存在" });
            }

            // 验证父权限是否存在
            if (createPermissionDto.ParentId.HasValue)
            {
                var parentExists = await _context.Permissions
                    .AnyAsync(p => p.Id == createPermissionDto.ParentId.Value);
                if (!parentExists)
                {
                    return BadRequest(new { message = "父权限不存在" });
                }
            }

            var permission = new Permission
            {
                Name = createPermissionDto.Name,
                Code = createPermissionDto.Code,
                ResourceType = Enum.Parse<ResourceType>(createPermissionDto.ResourceType!),
                ResourcePath = createPermissionDto.ResourcePath ?? string.Empty,
                ParentId = createPermissionDto.ParentId!
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("权限 {PermissionName} 创建成功", createPermissionDto.Name);

            var permissionDto = new PermissionDetailDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                ResourceType = permission.ResourceType.ToString(),
                ResourcePath = permission.ResourcePath,
                ParentId = permission.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permissionDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PermissionDetailDto>> UpdatePermission(Guid id, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = "权限不存在" });
            }

            // 检查权限代码是否已被其他权限使用
            if (!string.IsNullOrEmpty(updatePermissionDto.Code))
            {
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == updatePermissionDto.Code && p.Id != id);
                if (existingPermission != null)
                {
                    return BadRequest(new { message = "权限代码已存在" });
                }
                permission.Code = updatePermissionDto.Code;
            }

            if (!string.IsNullOrEmpty(updatePermissionDto.Name))
            {
                permission.Name = updatePermissionDto.Name;
            }

            if (!string.IsNullOrEmpty(updatePermissionDto.ResourceType))
            {
                permission.ResourceType = Enum.Parse<ResourceType>(updatePermissionDto.ResourceType!);
            }

            permission.ResourcePath = updatePermissionDto.ResourcePath ?? string.Empty;
            permission.ParentId = updatePermissionDto.ParentId!;

            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();

            var parent = permission.ParentId.HasValue ? 
                await _context.Permissions.FindAsync(permission.ParentId) : null;

            var permissionDto = new PermissionDetailDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                ResourceType = permission.ResourceType.ToString(),
                ResourcePath = permission.ResourcePath,
                ParentId = permission.ParentId,
                ParentName = parent?.Name,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(permissionDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePermission(Guid id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = "权限不存在" });
            }

            // 检查是否有子权限
            var hasChildren = await _context.Permissions
                .AnyAsync(p => p.ParentId == id);
            if (hasChildren)
            {
                return BadRequest(new { message = "该权限下有子权限，无法删除" });
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("权限 {PermissionName} 删除成功", permission.Name);
            return Ok(new { message = "权限删除成功" });
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
                Children = permission.Children != null ? BuildPermissionTree(permission.Children.ToList()) : new List<PermissionListDto>()
            }).ToList();
        }
    }
}