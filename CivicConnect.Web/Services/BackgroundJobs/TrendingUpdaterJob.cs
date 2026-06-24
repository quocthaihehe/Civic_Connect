using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services.BackgroundJobs
{
    public class TrendingUpdaterJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TrendingUpdaterJob> _logger;

        public TrendingUpdaterJob(IServiceProvider serviceProvider, ILogger<TrendingUpdaterJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TrendingUpdaterJob đang khởi chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var pointsService = scope.ServiceProvider.GetRequiredService<ICitizenPointsService>();

                        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                        // 1. Cập nhật TrendingTopics
                        // Lấy tất cả bài viết Approved trong 7 ngày
                        var recentPosts = await context.ForumPosts
                            .Where(p => p.Status == Models.Enums.PostStatus.Approved && p.CreatedAt >= sevenDaysAgo)
                            .ToListAsync(stoppingToken);

                        // Phân tích Tags
                        var tagsCount = recentPosts
                            .Where(p => !string.IsNullOrEmpty(p.Tags))
                            .SelectMany(p => p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            .Select(t => t.Trim().ToLower())
                            .GroupBy(t => t)
                            .Select(g => new { Tag = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(10) // Lấy top 10 tag
                            .ToList();

                        // Xoá tag cũ
                        var existingTopics = await context.TrendingTopics.ToListAsync(stoppingToken);
                        context.TrendingTopics.RemoveRange(existingTopics);

                        // Thêm tag mới
                        foreach (var tag in tagsCount)
                        {
                            context.TrendingTopics.Add(new TrendingTopic
                            {
                                Tag = tag.Tag,
                                PostCount = tag.Count,
                                LastUpdated = DateTime.UtcNow
                            });
                        }

                        // 2. Tính điểm PopularityScore cho các bài viết và tặng điểm Trending
                        var allActivePosts = await context.ForumPosts
                            .Where(p => p.Status == Models.Enums.PostStatus.Approved)
                            .ToListAsync(stoppingToken);

                        foreach (var post in allActivePosts)
                        {
                            // Tính điểm
                            post.PopularityScore = (post.LikeCount * 1.0f) + (post.CommentCount * 0.5f);
                            
                            // Tặng điểm nếu điểm độ hot >= 20 và chưa từng vào trending
                            if (post.PopularityScore >= 20.0f && post.EnteredTrendingAt == null)
                            {
                                post.EnteredTrendingAt = DateTime.UtcNow;
                                await pointsService.AwardPointsAsync(post.AuthorId, 20, 5, $"Bài viết '{post.Title}' lọt vào tab Được quan tâm.");
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("TrendingUpdaterJob đã chạy thành công lúc: {time}", DateTimeOffset.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong TrendingUpdaterJob");
                }

                // Đợi 30 phút trước khi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
