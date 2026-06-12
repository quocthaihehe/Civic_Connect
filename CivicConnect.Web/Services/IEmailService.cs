using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
