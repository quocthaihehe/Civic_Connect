using CivicConnect.Web.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var portStr = _configuration["EmailSettings:Port"] ?? "587";
            int.TryParse(portStr, out int port);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            // Nếu không cấu hình email gửi, ghi log Mock ra màn hình console để debug
            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                _logger.LogWarning($"[MOCK EMAIL] Gửi tới: {toEmail} | Tiêu đề: {subject}\nNội dung: {body}");
                return;
            }

            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail, "CivicConnect");
                    mail.To.Add(new MailAddress(toEmail));
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(smtpServer, port))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }
                }
                _logger.LogInformation($"[SMTP EMAIL] Đã gửi email thành công tới {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SMTP EMAIL ERROR] Thất bại khi gửi email tới {toEmail}: {ex.Message}");
                // Ghi lại nội dung vào log để lập trình viên vẫn lấy được mã OTP test khi bị lỗi kết nối SMTP
                _logger.LogWarning($"[MOCK FALLBACK] OTP của {toEmail} là: {body}");
                throw new Exception($"Không thể kết nối đến máy chủ SMTP để gửi email. Lỗi: {ex.Message}");
            }
        }
    }
}
