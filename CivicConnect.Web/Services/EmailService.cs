using CivicConnect.Web.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
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
            _logger.LogInformation($"[MOCK EMAIL] Gá»­i tá»›i {toEmail} | TiÃªu Ä‘á»: {subject}\nNá»™i dung: {body}");
            return Task.CompletedTask;
        }
    }
}

