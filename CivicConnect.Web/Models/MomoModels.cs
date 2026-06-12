using System;

namespace CivicConnect.Web.Models
{
    public class MomoOptionModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string QueryUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
    }

    public class MomoCreatePaymentResponseModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string Deeplink { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class MomoQueryResponseModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public long Amount { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public long TransId { get; set; }
        public long ResponseTime { get; set; }
        public string PaymentFileType { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class DonationOrderModel
    {
        public int CategoryId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string PaymentMethod { get; set; } = "captureWallet";
    }
}
