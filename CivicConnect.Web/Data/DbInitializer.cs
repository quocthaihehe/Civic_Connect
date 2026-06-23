using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public static async Task SeedCommunityDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            var admin = await userManager.FindByEmailAsync("admin@gmail.com");
            var citizen = await userManager.FindByEmailAsync("citizen@gmail.com");
            if (admin == null || citizen == null) return;

            // Seed Forum Posts
            if (!await context.ForumPosts.AnyAsync())
            {
                var posts = new List<ForumPost>
                {
                    new ForumPost { Title = "Quy định mới về phân loại rác thải tại nguồn", Content = "Từ ngày 01/01/2027, hộ gia đình không phân loại rác sẽ bị phạt từ 500k - 1 triệu đồng.", Tags = "moitruong, quy_dinh", AuthorId = admin.Id, Status = CivicConnect.Web.Models.Enums.PostStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-2), LikeCount = 15, CommentCount = 2, PopularityScore = 16f, EnteredTrendingAt = DateTime.UtcNow },
                    new ForumPost { Title = "Khen ngợi đội ngũ xử lý đường ngập Quận 7", Content = "Hôm qua mưa lớn ngập đường Huỳnh Tấn Phát, nhưng sáng nay nước đã rút nhanh. Cảm ơn các anh cán bộ.", Tags = "giaothong, khen_ngoi", AuthorId = citizen.Id, Status = CivicConnect.Web.Models.Enums.PostStatus.Approved, CreatedAt = DateTime.UtcNow.AddHours(-5), LikeCount = 35, CommentCount = 5, PopularityScore = 37.5f, EnteredTrendingAt = DateTime.UtcNow },
                    new ForumPost { Title = "Hỏi về thủ tục đăng ký tạm trú online", Content = "Mọi người cho em hỏi làm tạm trú trên app VNeID mất bao lâu thì được duyệt ạ?", Tags = "thu_tuc, hanh_chinh", AuthorId = citizen.Id, Status = CivicConnect.Web.Models.Enums.PostStatus.Approved, CreatedAt = DateTime.UtcNow.AddHours(-1) }
                };
                context.ForumPosts.AddRange(posts);
                await context.SaveChangesAsync();

                // Seed Comments
                var comment1 = new ForumComment { PostId = posts[1].Id, Content = "Tuyệt vời quá bạn ơi!", AuthorId = admin.Id, CreatedAt = DateTime.UtcNow.AddHours(-4) };
                context.ForumComments.Add(comment1);
                await context.SaveChangesAsync();

                var reply1 = new ForumComment { PostId = posts[1].Id, ParentCommentId = comment1.Id, Content = "Dạ cảm ơn admin", AuthorId = citizen.Id, CreatedAt = DateTime.UtcNow.AddHours(-3), Depth = 1 };
                context.ForumComments.Add(reply1);
                await context.SaveChangesAsync();
            }

            // Seed Polls
            if (!await context.Polls.AnyAsync())
            {
                var poll = new Poll
                {
                    Question = "Bạn thấy tính năng nào cần thiết nhất cho phiên bản tới?",
                    Description = "Giúp ban quản trị ưu tiên phát triển ứng dụng",
                    IsActive = true,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Options = new List<PollOption>
                    {
                        new PollOption { Text = "Bản đồ nhiệt độ giao thông" },
                        new PollOption { Text = "Cảnh báo ngập lụt realtime" },
                        new PollOption { Text = "Tra cứu vi phạm giao thông" }
                    }
                };
                context.Polls.Add(poll);
            }

            // Seed Petitions
            if (!await context.Petitions.AnyAsync())
            {
                context.Petitions.Add(new Petition
                {
                    Title = "Kiến nghị mở rộng tuyến xe buýt số 150",
                    Description = "Tuyến xe buýt 150 thường xuyên quá tải vào giờ cao điểm, cần tăng chuyến.",
                    TargetSignatures = 1000,
                    CurrentSignatures = 150,
                    EndDate = DateTime.UtcNow.AddDays(30)
                });
            }

            // Seed Events
            if (!await context.CommunityEvents.AnyAsync())
            {
                context.CommunityEvents.Add(new CommunityEvent
                {
                    Title = "Ngày hội Trồng cây xanh Quận 7",
                    Description = "Tham gia phủ xanh tuyến đường Nguyễn Hữu Thọ.",
                    Location = "Đường Nguyễn Hữu Thọ, Quận 7",
                    StartTime = DateTime.UtcNow.AddDays(5),
                    EndTime = DateTime.UtcNow.AddDays(5).AddHours(4),
                    MaxParticipants = 200
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
