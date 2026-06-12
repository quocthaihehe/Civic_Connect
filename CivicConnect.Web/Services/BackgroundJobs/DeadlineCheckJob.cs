using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services.BackgroundJobs
{
    public class DeadlineCheckJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeadlineCheckJob> _logger;

        public DeadlineCheckJob(IServiceProvider serviceProvider, ILogger<DeadlineCheckJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DeadlineCheckJob background task is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var issueService = scope.ServiceProvider.GetRequiredService<IIssueService>();
                        _logger.LogInformation("DeadlineCheckJob is executing.");
                        await issueService.CheckDeadlinesAsync();
                        _logger.LogInformation("DeadlineCheckJob executed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing DeadlineCheckJob.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("DeadlineCheckJob background task is stopping.");
        }
    }
}

