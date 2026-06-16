using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class MomoService : IMomoService
    {
        private readonly MomoOptionModel _momoOptions;

        public MomoService(IConfiguration configuration)
        {
            _momoOptions = new MomoOptionModel
            {
                PartnerCode = configuration["MomoAPI:PartnerCode"] ?? "",
                AccessKey = configuration["MomoAPI:AccessKey"] ?? "",
                SecretKey = configuration["MomoAPI:SecretKey"] ?? "",
                PaymentUrl = configuration["MomoAPI:PaymentUrl"] ?? "",
                QueryUrl = configuration["MomoAPI:QueryUrl"] ?? "",
                ReturnUrl = configuration["MomoAPI:ReturnUrl"] ?? "",
                NotifyUrl = configuration["MomoAPI:NotifyUrl"] ?? ""
            };
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(DonationCategory category, Donation donation, string paymentMethod = "captureWallet")
        {
            var requestId = donation.OrderId;
            var orderId = donation.OrderId;
            var amount = (long)donation.Amount;
            var orderInfo = $"Quyên góp {category.Name}";
            var extraData = "";
            var requestType = string.IsNullOrEmpty(paymentMethod) ? "captureWallet" : paymentMethod;

            var rawHash = $"accessKey={_momoOptions.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={_momoOptions.NotifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_momoOptions.PartnerCode}&redirectUrl={_momoOptions.ReturnUrl}&requestId={requestId}&requestType={requestType}";
            
            var signature = SignSha256(rawHash, _momoOptions.SecretKey);

            var requestData = new
            {
                partnerCode = _momoOptions.PartnerCode,
                partnerName = "CivicConnect Fund",
                storeId = "CivicConnectStore",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = _momoOptions.ReturnUrl,
                ipnUrl = _momoOptions.NotifyUrl,
                requestType = requestType,
                extraData = extraData,
                lang = "vi",
                signature = signature
            };

            using (var client = new HttpClient())
            {
                var serializeOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                var json = JsonSerializer.Serialize(requestData, serializeOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_momoOptions.PaymentUrl, content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<MomoCreatePaymentResponseModel>(responseContent, options) 
                           ?? new MomoCreatePaymentResponseModel { ResultCode = -99, Message = "Deserialization failed" };
                }
                else
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errModel = JsonSerializer.Deserialize<MomoCreatePaymentResponseModel>(responseContent, options);
                        if (errModel != null && !string.IsNullOrEmpty(errModel.Message))
                        {
                            return errModel;
                        }
                    }
                    catch { }

                    return new MomoCreatePaymentResponseModel
                    {
                        ResultCode = (int)response.StatusCode,
                        Message = $"HTTP error {(int)response.StatusCode}: {response.ReasonPhrase}. Response: {responseContent}"
                    };
                }
            }
        }

        public async Task<MomoQueryResponseModel> QueryPaymentStatusAsync(string orderId)
        {
            var requestId = Guid.NewGuid().ToString();
            var rawHash = $"accessKey={_momoOptions.AccessKey}&orderId={orderId}&partnerCode={_momoOptions.PartnerCode}&requestId={requestId}";
            
            var signature = SignSha256(rawHash, _momoOptions.SecretKey);

            var requestData = new
            {
                partnerCode = _momoOptions.PartnerCode,
                requestId = requestId,
                orderId = orderId,
                signature = signature
            };

            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_momoOptions.QueryUrl, content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<MomoQueryResponseModel>(responseContent, options) 
                           ?? new MomoQueryResponseModel { ResultCode = -99, Message = "Deserialization failed" };
                }
                else
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errModel = JsonSerializer.Deserialize<MomoQueryResponseModel>(responseContent, options);
                        if (errModel != null && !string.IsNullOrEmpty(errModel.Message))
                        {
                            return errModel;
                        }
                    }
                    catch { }

                    return new MomoQueryResponseModel
                    {
                        ResultCode = (int)response.StatusCode,
                        Message = $"HTTP error {(int)response.StatusCode}: {response.ReasonPhrase}. Response: {responseContent}"
                    };
                }
            }
        }

        private string SignSha256(string message, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return string.Concat(Array.ConvertAll(hashmessage, x => x.ToString("x2")));
            }
        }
    }
}

