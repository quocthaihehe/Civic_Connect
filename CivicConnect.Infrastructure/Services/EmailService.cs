using CivicConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CivicConnect.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation($"[MOCK EMAIL] Gửi tới {toEmail} | Tiêu đề: {subject}\nNội dung: {body}");
            return Task.CompletedTask;
        }
    }
}
