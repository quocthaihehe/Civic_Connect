using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}
