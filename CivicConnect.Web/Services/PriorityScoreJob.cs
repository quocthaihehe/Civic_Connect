using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class PriorityScoreJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriorityScoreJob> _logger;

        public PriorityScoreJob(IServiceProvider serviceProvider, ILogger<PriorityScoreJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriorityScoreJob background task is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var issueService = scope.ServiceProvider.GetRequiredService<IIssueService>();
                        _logger.LogInformation("PriorityScoreJob is executing.");
                        await issueService.UpdatePriorityScoresAsync();
                        _logger.LogInformation("PriorityScoreJob executed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing PriorityScoreJob.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("PriorityScoreJob background task is stopping.");
        }
    }
}
