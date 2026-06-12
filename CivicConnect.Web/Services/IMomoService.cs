using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Momo;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(DonationCategory category, Donation donation, string paymentMethod = "captureWallet");
        Task<MomoQueryResponseModel> QueryPaymentStatusAsync(string orderId);
    }
}
