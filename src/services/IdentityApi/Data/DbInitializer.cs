using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            AuthDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            // 创建角色
            string[] roles = { "SystemAdmin", "SupplierAdmin", "SupplierUser", "BuyerAdmin", "BuyerUser", "Guest" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleType = roleName switch
                    {
                        "SystemAdmin" => RoleType.System,
                        "SupplierAdmin" or "SupplierUser" => RoleType.Supplier,
                        "BuyerAdmin" or "BuyerUser" => RoleType.Buyer,
                        _ => RoleType.Custom
                    };

                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role",
                        RoleType = roleType
                    });
                }
            }

            // 创建默认管理员用户
            var adminUser = await userManager.FindByEmailAsync("admin@openbearing.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@openbearing.com",
                    Email = "admin@openbearing.com",
                    DisplayName = "系统管理员",
                    UserType = UserType.SystemAdmin,
                    EmailConfirmed = true,
                    IsActive = true,
                    IsVerified = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                }
            }
        }
    }
}
