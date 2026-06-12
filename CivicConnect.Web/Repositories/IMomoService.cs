using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models;
using System.Threading.Tasks;

namespace CivicConnect.Web.Repositories
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(DonationCategory category, Donation donation, string paymentMethod = "captureWallet");
        Task<MomoQueryResponseModel> QueryPaymentStatusAsync(string orderId);
    }
}

