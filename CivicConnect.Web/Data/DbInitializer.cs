using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CivicConnect.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var defaultPassword = "Admin@123456";

            // 1. Seed Quản trị viên
            if (await userManager.FindByEmailAsync("admin@gmail.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    FullName = "Quản Trị Viên Hệ Thống",
                    IsEmailVerified = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 2. Seed Cán bộ Phường (Phường Bến Nghé, Quận 1)
            if (await userManager.FindByEmailAsync("canbo.phuong@gmail.com") == null)
            {
                var officialWard = new ApplicationUser
                {
                    UserName = "canbo.phuong@gmail.com",
                    Email = "canbo.phuong@gmail.com",
                    FullName = "Cán Bộ Phường Bến Nghé",
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    WardCode = "26734",
                    IsEmailVerified = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    GovernmentUnitId = "UBND_BENNGHE",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(officialWard, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(officialWard, "OfficialWard");
                }
            }

            // 3. Seed Cán bộ Quận (Quận 1)
            if (await userManager.FindByEmailAsync("canbo.quan@gmail.com") == null)
            {
                var officialDistrict = new ApplicationUser
                {
                    UserName = "canbo.quan@gmail.com",
                    Email = "canbo.quan@gmail.com",
                    FullName = "Cán Bộ UBND Quận 1",
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    IsEmailVerified = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    GovernmentUnitId = "UBND_Q1",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(officialDistrict, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(officialDistrict, "OfficialDistrict");
                }
            }

            // 4. Seed Công dân
            if (await userManager.FindByEmailAsync("citizen@gmail.com") == null)
            {
                var citizen = new ApplicationUser
                {
                    UserName = "citizen@gmail.com",
                    Email = "citizen@gmail.com",
                    FullName = "Nguyễn Văn Công Dân",
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    WardCode = "26734",
                    IsEmailVerified = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(citizen, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(citizen, "Citizen");
                }
            }
        }
    }
}
