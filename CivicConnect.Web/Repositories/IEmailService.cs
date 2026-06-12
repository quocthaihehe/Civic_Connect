using System.Threading.Tasks;

namespace CivicConnect.Web.Repositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}

