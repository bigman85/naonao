using HHPortal.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<IdentityRole<Guid>> roleManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleListDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = roles.Select(role => new RoleListDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return Ok(roleDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDetailDto>> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var roleDto = new RoleDetailDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(roleDto);
        }

        [HttpPost]
        public async Task<ActionResult<RoleListDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 检查角色名称是否已存在
            var existingRole = await _roleManager.FindByNameAsync(createRoleDto.Name);
            if (existingRole != null)
            {
                return BadRequest(new { message = "角色名称已存在" });
            }

            var role = new IdentityRole<Guid>
            {
                Name = createRoleDto.Name,
                NormalizedName = createRoleDto.Name.ToUpper()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "创建角色失败", errors = result.Errors });
            }

            _logger.LogInformation("角色 {RoleName} 创建成功", createRoleDto.Name);

            var roleDto = new RoleListDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoleListDto>> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "更新角色失败", errors = result.Errors });
            }

            var roleDto = new RoleListDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(roleDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "删除角色失败", errors = result.Errors });
            }

            _logger.LogInformation("角色 {RoleName} 删除成功", role.Name);
            return Ok(new { message = "角色删除成功" });
        }
    }
}