using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class SlaCheckerService : BackgroundService
    {
        private readonly ILogger<SlaCheckerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SlaCheckerService(ILogger<SlaCheckerService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SlaCheckerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckSlaBreachesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing SlaCheckerService.");
                }

                // Run every 1 hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("SlaCheckerService is stopping.");
        }

        private async Task CheckSlaBreachesAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var now = DateTime.UtcNow;

                var breachedIssues = await context.Issues
                    .Where(i => (i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned) 
                                && i.DueDate.HasValue 
                                && i.DueDate.Value < now)
                    .ToListAsync();

                if (!breachedIssues.Any()) return;

                int count = 0;
                foreach (var issue in breachedIssues)
                {
                    // Check if already breached to avoid duplicate logs every hour
                    var alreadyLogged = await context.IssueStatusHistories
                        .AnyAsync(h => h.IssueId == issue.Id && h.Note.Contains("[SLA_BREACH]"));
                        
                    if (!alreadyLogged)
                    {
                        var history = new IssueStatusHistory
                        {
                            IssueId = issue.Id,
                            FromStatus = issue.Status,
                            ToStatus = issue.Status,
                            ChangedById = "System",
                            Note = "[SLA_BREACH] Phản ánh đã quá hạn xử lý theo quy định.",
                            ChangedAt = now
                        };
                        context.IssueStatusHistories.Add(history);

                        if (!string.IsNullOrEmpty(issue.AssignedToUserId))
                        {
                            await notificationService.SendNotificationAsync(
                                issue.AssignedToUserId,
                                "Cảnh báo: Phản ánh quá hạn SLA",
                                $"Phản ánh '{issue.Title}' mà bạn đang phụ trách đã quá hạn xử lý. Vui lòng cập nhật tiến độ ngay.",
                                NotificationType.SystemAlert,
                                issue.Id.ToString()
                            );
                        }
                        
                        count++;
                    }
                }

                if (count > 0)
                {
                    await context.SaveChangesAsync();
                    _logger.LogWarning($"[SLA_BREACH] Logged {count} SLA breaches.");
                }
            }
        }
    }
}
