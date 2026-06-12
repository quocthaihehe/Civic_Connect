using System.Threading.Tasks;

namespace CivicConnect.Web.Repositories
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}

