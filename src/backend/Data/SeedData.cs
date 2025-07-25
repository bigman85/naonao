using HHPortal.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HHPortal.Backend.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            // 应用数据库迁移
            await context.Database.MigrateAsync();

            // 初始化权限
            await InitializePermissions(context);

            // 初始化角色
            await InitializeRoles(context, roleManager);

            // 初始化管理员用户
            await InitializeAdminUser(userManager, roleManager);
        }

        private static async Task InitializePermissions(ApplicationDbContext context)
        {
            if (await context.Permissions.AnyAsync())
            {
                return; // 权限已存在
            }

            var permissions = new List<Permission>
            {
                // 系统管理权限
                new Permission { Name = "系统管理", Code = "system.manage", ResourceType = ResourceType.Menu },
                
                // 用户管理权限
                new Permission { Name = "用户管理", Code = "user.manage", ResourceType = ResourceType.Menu },
                new Permission { Name = "查看用户", Code = "user.view", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "创建用户", Code = "user.create", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "编辑用户", Code = "user.edit", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "删除用户", Code = "user.delete", ResourceType = ResourceType.Button, ParentId = null },
                
                // 角色管理权限
                new Permission { Name = "角色管理", Code = "role.manage", ResourceType = ResourceType.Menu },
                new Permission { Name = "查看角色", Code = "role.view", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "创建角色", Code = "role.create", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "编辑角色", Code = "role.edit", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "删除角色", Code = "role.delete", ResourceType = ResourceType.Button, ParentId = null },
                
                // 权限管理权限
                new Permission { Name = "权限管理", Code = "permission.manage", ResourceType = ResourceType.Menu },
                new Permission { Name = "查看权限", Code = "permission.view", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "创建权限", Code = "permission.create", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "编辑权限", Code = "permission.edit", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "删除权限", Code = "permission.delete", ResourceType = ResourceType.Button, ParentId = null },
                
                // 内容管理权限
                new Permission { Name = "内容管理", Code = "content.manage", ResourceType = ResourceType.Menu },
                new Permission { Name = "查看内容", Code = "content.view", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "创建内容", Code = "content.create", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "编辑内容", Code = "content.edit", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "删除内容", Code = "content.delete", ResourceType = ResourceType.Button, ParentId = null },
                
                // 文件管理权限
                new Permission { Name = "文件管理", Code = "file.manage", ResourceType = ResourceType.Menu },
                new Permission { Name = "上传文件", Code = "file.upload", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "下载文件", Code = "file.download", ResourceType = ResourceType.Button, ParentId = null },
                new Permission { Name = "删除文件", Code = "file.delete", ResourceType = ResourceType.Button, ParentId = null }
            };

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }

        private static async Task InitializeRoles(ApplicationDbContext context, RoleManager<Role> roleManager)
        {
            var roles = new[]
            {
                "Admin",
                "Manager",
                "User"
            };

            foreach (var roleName in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new Role { Name = roleName };
                    await roleManager.CreateAsync(role);
                }
            }

            // 为管理员角色分配所有权限
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
            {
                var permissions = await context.Permissions.ToListAsync();
                var existingRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var newRolePermissions = permissions
                    .Where(p => !existingRolePermissions.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = p.Id
                    });

                context.RolePermissions.AddRange(newRolePermissions);
                await context.SaveChangesAsync();
            }
        }

        private static async Task InitializeAdminUser(
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@hhportal.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}