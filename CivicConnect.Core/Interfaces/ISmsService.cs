using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}
