using CivicConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CivicConnect.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation($"[MOCK SMS] Gửi tới {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }
}
