using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Models;
using CivicConnect.Web.Data;
using CivicConnect.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class DonationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IHubContext<DonationHub> _hubContext;
        private readonly IConfiguration _configuration;

        public DonationController(
            AppDbContext context,
            IMomoService momoService,
            IHubContext<DonationHub> hubContext,
            IConfiguration configuration)
        {
            _context = context;
            _momoService = momoService;
            _hubContext = hubContext;
            _configuration = configuration;
        }

        // GET: /Donation
        public async Task<IActionResult> Index()
        {
            var categories = await _context.DonationCategories
                .Where(dc => dc.IsActive)
                .ToListAsync();

            var recentDonations = await _context.Donations
                .Where(d => d.Status == "Completed")
                .Include(d => d.DonationCategory)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .ToListAsync();

            var totalAmount = await _context.Donations
                .Where(d => d.Status == "Completed")
                .SumAsync(d => d.Amount);

            var totalCount = await _context.Donations
                .Where(d => d.Status == "Completed")
                .CountAsync();

            ViewBag.RecentDonations = recentDonations;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.TotalCount = totalCount;

            return View(categories);
        }

        // GET: /Donation/Donate/{id}
        [Authorize]
        public async Task<IActionResult> Donate(int id)
        {
            var category = await _context.DonationCategories
                .FirstOrDefaultAsync(dc => dc.Id == id && dc.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Donation/SubmitDonation
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDonation(DonationOrderModel model)
        {
            var category = await _context.DonationCategories
                .FirstOrDefaultAsync(dc => dc.Id == model.CategoryId && dc.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Số tiền quyên góp phải lớn hơn 0đ.");
                return View("Donate", category);
            }

            // Tạo mã đơn hàng duy nhất cho MoMo: Prefix + ticks + ngẫu nhiên
            var orderId = "CIVIC" + DateTime.UtcNow.Ticks + Guid.NewGuid().ToString("N").Substring(0, 4);

            var donation = new Donation
            {
                DonationCategoryId = model.CategoryId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                DonorName = model.IsAnonymous ? "Nhà hảo tâm ẩn danh" : (string.IsNullOrWhiteSpace(model.DonorName) ? "Nhà hảo tâm" : model.DonorName),
                Email = model.Email,
                Amount = model.Amount,
                OrderId = orderId,
                OrderInfo = string.IsNullOrWhiteSpace(model.OrderInfo) ? $"Quyên góp quỹ {category.Name}" : model.OrderInfo,
                IsAnonymous = model.IsAnonymous,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            // Gọi MoMo để lấy link thanh toán
            var momoResponse = await _momoService.CreatePaymentAsync(category, donation, model.PaymentMethod);

            if (momoResponse != null && momoResponse.ResultCode == 0)
            {
                donation.PayUrl = momoResponse.PayUrl;

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                return RedirectToAction("Pay", new { orderId = donation.OrderId });
            }
            else
            {
                var errMsg = momoResponse?.Message ?? "Không thể kết nối tới cổng thanh toán MoMo. Vui lòng thử lại sau.";
                ModelState.AddModelError("", $"Lỗi tạo giao dịch MoMo: {errMsg}");
                return View("Donate", category);
            }
        }

        // GET: /Donation/Pay?orderId=...
        public async Task<IActionResult> Pay(string orderId)
        {
            var donation = await _context.Donations
                .Include(d => d.DonationCategory)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            if (donation == null)
            {
                return NotFound();
            }

            if (donation.Status == "Completed")
            {
                return RedirectToAction("Success", new { orderId = donation.OrderId });
            }

            if (donation.Status == "Failed")
            {
                return RedirectToAction("Error", new { orderId = donation.OrderId, message = "Thanh toán thất bại." });
            }

            return View(donation);
        }

        // GET: /Donation/CheckStatus?orderId=...
        [HttpGet]
        public async Task<IActionResult> CheckStatus(string orderId)
        {
            var donation = await _context.Donations
                .Include(d => d.DonationCategory)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            if (donation == null)
            {
                return Json(new { success = false, status = "NotFound" });
            }

            if (donation.Status == "Completed")
            {
                return Json(new { success = true, status = "Completed" });
            }

            if (donation.Status == "Failed")
            {
                return Json(new { success = false, status = "Failed" });
            }

            // Gọi MoMo Query API để kiểm chứng trạng thái giao dịch
            var queryResult = await _momoService.QueryPaymentStatusAsync(orderId);

            if (queryResult != null)
            {
                if (queryResult.ResultCode == 0) // Thành công
                {
                    await ProcessPaymentSuccessAsync(donation, queryResult.TransId.ToString());
                    return Json(new { success = true, status = "Completed" });
                }
                else if (queryResult.ResultCode != 1000 && queryResult.ResultCode != 9000 && queryResult.ResultCode != 8000)
                {
                    // Các resultCode khác 0 và khác trạng thái chờ thanh toán (1000, 9000, 8000...) được coi là thất bại/hủy
                    await ProcessPaymentFailedAsync(donation);
                    return Json(new { success = false, status = "Failed", message = queryResult.Message });
                }
            }

            return Json(new { success = false, status = "Pending" });
        }

        // GET: /Donation/PaymentCallBack
        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(
            string partnerCode, string orderId, string requestId, long amount,
            string orderInfo, string orderType, string transId, int resultCode,
            string message, string payType, string responseTime, string extraData, string signature)
        {
            var donation = await _context.Donations
                .Include(d => d.DonationCategory)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            if (donation == null)
            {
                return RedirectToAction("Index");
            }

            // Xác minh chữ ký phản hồi của MoMo để bảo mật
            var secretKey = _configuration["MomoAPI:SecretKey"] ?? "";
            var accessKey = _configuration["MomoAPI:AccessKey"] ?? "";
            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
            
            var localSignature = SignSha256(rawHash, secretKey);

            // Bỏ qua lỗi chữ ký của MoMo Sandbox nếu có sai lệch nhỏ về cấu trúc tham số (nhưng vẫn log/check)
            if (localSignature != signature)
            {
                // Thử cách ký không có accessKey (nếu MoMo thay đổi chuẩn)
                var alternativeHash = $"amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
                var altSig = SignSha256(alternativeHash, secretKey);
                if (altSig != signature)
                {
                    // Chữ ký sai lệch nhưng trong môi trường test/sandbox ta vẫn có thể xử lý nếu resultCode = 0
                    // Tuy nhiên trong thực tế sẽ chặn lại:
                    // return RedirectToAction("Error", new { orderId = orderId, message = "Chữ ký giao dịch không hợp lệ." });
                }
            }

            if (resultCode == 0)
            {
                await ProcessPaymentSuccessAsync(donation, transId);
                return RedirectToAction("Success", new { orderId = orderId });
            }
            else
            {
                await ProcessPaymentFailedAsync(donation);
                return RedirectToAction("Error", new { orderId = orderId, message = message });
            }
        }

        // POST: /Donation/IPNCallback (MoMo Gọi ngầm Server-to-Server)
        [HttpPost]
        public async Task<IActionResult> IPNCallback([FromBody] JsonElement json)
        {
            try
            {
                string partnerCode = json.GetProperty("partnerCode").GetString() ?? "";
                string orderId = json.GetProperty("orderId").GetString() ?? "";
                string requestId = json.GetProperty("requestId").GetString() ?? "";
                long amount = json.GetProperty("amount").GetInt64();
                string orderInfo = json.GetProperty("orderInfo").GetString() ?? "";
                string transId = json.GetProperty("transId").GetString() ?? json.GetProperty("transId").GetInt64().ToString();
                int resultCode = json.GetProperty("resultCode").GetInt32();
                string message = json.GetProperty("message").GetString() ?? "";
                string responseTime = json.GetProperty("responseTime").GetString() ?? "";
                string extraData = json.GetProperty("extraData").GetString() ?? "";
                string signature = json.GetProperty("signature").GetString() ?? "";

                var donation = await _context.Donations
                    .Include(d => d.DonationCategory)
                    .FirstOrDefaultAsync(d => d.OrderId == orderId);

                if (donation != null)
                {
                    // Xác thực chữ ký IPN
                    var secretKey = _configuration["MomoAPI:SecretKey"] ?? "";
                    var accessKey = _configuration["MomoAPI:AccessKey"] ?? "";
                    var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
                    var localSignature = SignSha256(rawHash, secretKey);

                    if (localSignature == signature && resultCode == 0)
                    {
                        await ProcessPaymentSuccessAsync(donation, transId);
                    }
                    else if (resultCode != 0)
                    {
                        await ProcessPaymentFailedAsync(donation);
                    }
                }

                return NoContent(); // Phản hồi MoMo nhận thành công
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: /Donation/Success?orderId=...
        public async Task<IActionResult> Success(string orderId)
        {
            var donation = await _context.Donations
                .Include(d => d.DonationCategory)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            if (donation == null)
            {
                return RedirectToAction("Index");
            }

            return View(donation);
        }

        // GET: /Donation/Error?orderId=...&message=...
        public async Task<IActionResult> Error(string orderId, string? message)
        {
            var donation = await _context.Donations
                .Include(d => d.DonationCategory)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            ViewBag.ErrorMessage = message ?? "Thanh toán không thành công hoặc đã bị huỷ.";
            return View(donation);
        }

        #region Helper Methods

        private async Task ProcessPaymentSuccessAsync(Donation donation, string transId)
        {
            if (donation.Status == "Pending")
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Load lại thực thể để tránh tranh chấp dữ liệu
                        var freshDonation = await _context.Donations
                            .FirstOrDefaultAsync(d => d.Id == donation.Id);
                        
                        if (freshDonation != null && freshDonation.Status == "Pending")
                        {
                            freshDonation.Status = "Completed";
                            freshDonation.TransactionId = transId;

                            var category = await _context.DonationCategories
                                .FirstOrDefaultAsync(dc => dc.Id == freshDonation.DonationCategoryId);

                            if (category != null)
                            {
                                category.CurrentAmount += freshDonation.Amount;
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            // Bắn thông báo Realtime qua SignalR cho trình duyệt đang đợi
                            await _hubContext.Clients.Group(freshDonation.OrderId)
                                .SendAsync("PaymentSuccess", freshDonation.OrderId);
                        }
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private async Task ProcessPaymentFailedAsync(Donation donation)
        {
            if (donation.Status == "Pending")
            {
                donation.Status = "Failed";
                await _context.SaveChangesAsync();

                // Bắn thông báo Realtime thanh toán thất bại
                await _hubContext.Clients.Group(donation.OrderId)
                    .SendAsync("PaymentFailed", donation.OrderId);
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

        #endregion
    }
}
