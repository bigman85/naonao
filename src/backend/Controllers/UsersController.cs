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
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<IdentityUser<Guid>> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserListDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserListDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDetailDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = DateTime.UtcNow
            };

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 检查用户名是否已存在
            var existingUser = await _userManager.FindByNameAsync(createUserDto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "用户名已存在" });
            }

            // 检查邮箱是否已存在
            existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "邮箱已被使用" });
            }

            var user = new IdentityUser<Guid>
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Email,
                // 使用扩展方法设置创建时间
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "创建用户失败", errors = result.Errors });
            }

            // 分配角色
            if (createUserDto.Roles != null && createUserDto.Roles.Any())
            {
                foreach (var roleName in createUserDto.Roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }

            _logger.LogInformation("用户 {Username} 创建成功", createUserDto.Username);

            var userDto = new UserDetailDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = createUserDto.Roles ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDetailDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                // 检查邮箱是否已被其他用户使用
                var existingUser = await _userManager.FindByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { message = "邮箱已被使用" });
                }
                user.Email = updateUserDto.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "更新用户信息失败", errors = result.Errors });
            }

            // 更新角色
            if (updateUserDto.Roles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                foreach (var roleName in updateUserDto.Roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDetailDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                CreatedAt = DateTime.UtcNow
            };

            return Ok(userDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "删除用户失败", errors = result.Errors });
            }

            _logger.LogInformation("用户 {Username} 删除成功", user.UserName);
            return Ok(new { message = "用户删除成功" });
        }

        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "密码修改失败", errors = result.Errors });
            }

            _logger.LogInformation("用户 {Username} 密码修改成功", user.UserName);
            return Ok(new { message = "密码修改成功" });
        }
    }


}