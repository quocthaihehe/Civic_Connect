using CivicConnect.Core.Entities;
using CivicConnect.Core.Models;
using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(DonationCategory category, Donation donation, string paymentMethod = "captureWallet");
        Task<MomoQueryResponseModel> QueryPaymentStatusAsync(string orderId);
    }
}
