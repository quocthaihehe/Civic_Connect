using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Infrastructure.Data;
using CivicConnect.Core.Entities;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System;

namespace CivicConnect.Web.Controllers
{
    public class PolicyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PolicyController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageHeader"] = "Chính Sách & Tin Tức";
            ViewData["Title"] = "Chính Sách & Tin Tức";
            
            // Load policies from database
            var dbPolicies = await _context.Policies
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            // Fetch dynamic RSS news
            var rssNews = await FetchRssNewsAsync();

            // Combine list (DB policies first, then RSS news, both ordered by PublishedDate)
            var allPolicies = dbPolicies.Concat(rssNews)
                .OrderByDescending(p => p.PublishedDate)
                .ToList();

            return View(allPolicies);
        }

        public async Task<IActionResult> Details(int id, string url, string title, string excerpt)
        {
            if (id == 0 && !string.IsNullOrEmpty(url))
            {
                // Create dynamic Policy model for external RSS link
                var policy = new Policy
                {
                    Id = 0,
                    Title = string.IsNullOrEmpty(title) ? "Tin Tức Trực Tuyến" : Uri.UnescapeDataString(title),
                    Excerpt = string.IsNullOrEmpty(excerpt) ? "Tin tức cập nhật từ báo chí điện tử." : Uri.UnescapeDataString(excerpt),
                    Content = string.IsNullOrEmpty(excerpt) ? "Tin tức được liên kết trực tiếp từ cổng thông tin báo chí." : Uri.UnescapeDataString(excerpt),
                    SourceUrl = url,
                    Tag = "Tin tức",
                    TagClass = "tag-news",
                    IssuingUnit = "Báo điện tử",
                    PublishedDate = DateTime.UtcNow,
                    IsActive = true
                };

                ViewData["PageHeader"] = policy.Title;
                ViewData["Title"] = policy.Title;
                return View(policy);
            }

            var policyDb = await _context.Policies.FindAsync(id);
            if (policyDb == null || !policyDb.IsActive)
            {
                return NotFound();
            }

            ViewData["PageHeader"] = policyDb.Title;
            ViewData["Title"] = policyDb.Title;
            return View(policyDb);
        }

        [HttpGet]
        public async Task<IActionResult> Proxy(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Content("URL không hợp lệ.");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // Tối ưu hóa timeout là 4 giây để người dùng không phải chờ lâu
                    client.Timeout = TimeSpan.FromSeconds(4);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP Error {response.StatusCode}");
                    }
                    
                    var html = await response.Content.ReadAsStringAsync();
                    
                    // Set base tag to resolve relative paths
                    var uri = new Uri(url);
                    var baseHref = $"{uri.Scheme}://{uri.Host}";
                    var baseTag = $"<base href=\"{baseHref}/\" />";
                    
                    // Remove top location redirects to prevent escape
                    html = html.Replace("window.top.location", "window.self.location");
                    html = html.Replace("top.location", "self.location");
                    
                    if (html.Contains("<head>"))
                    {
                        html = html.Replace("<head>", $"<head>\n{baseTag}");
                    }
                    else if (html.Contains("<HEAD>"))
                    {
                        html = html.Replace("<HEAD>", $"<HEAD>\n{baseTag}");
                    }
                    else
                    {
                        html = baseTag + html;
                    }
                    
                    return Content(html, "text/html; charset=utf-8");
                }
            }
            catch (Exception)
            {
                // Trả về trang thông báo giao diện đẹp mắt nếu không load được iframe
                var fallbackHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css'>
    <style>
        body {{ font-family: system-ui, -apple-system, sans-serif; background-color: #f8f9fa; color: #495057; display: flex; align-items: center; justify-content: center; height: 90vh; margin: 0; padding: 20px; text-align: center; }}
        .card {{ background: white; border: 1px solid #dee2e6; border-radius: 16px; padding: 40px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); max-width: 480px; width: 100%; }}
        .icon-box {{ font-size: 3.5rem; color: #0d6efd; margin-bottom: 20px; }}
        h4 {{ font-weight: 800; color: #212529; margin-bottom: 12px; }}
        p {{ font-size: 0.925rem; line-height: 1.5; color: #6c757d; margin-bottom: 24px; }}
        .btn-open {{ font-weight: 700; padding: 10px 24px; border-radius: 8px; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon-box'><i class='bi bi-globe2'></i></div>
        <h4>Xem Trang Gốc Trực Tiếp</h4>
        <p>Máy chủ bị hạn chế hoặc đường truyền tới trang gốc quá chậm. Để xem đầy đủ và chi tiết nhất, vui lòng mở liên kết trực tiếp dưới đây.</p>
        <a href='{url}' target='_blank' class='btn btn-primary btn-open'><i class='bi bi-box-arrow-up-right me-1'></i> Mở Trang Gốc Trong Tab Mới</a>
    </div>
</body>
</html>";
                return Content(fallbackHtml, "text/html; charset=utf-8");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AskAI([FromBody] AskAIRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Question))
            {
                return Json(new { success = false, answer = "Câu hỏi hoặc dữ liệu không hợp lệ." });
            }

            string context = $"Tên văn bản/Chính sách: {request.DocTitle}\n" +
                             $"Nội dung tóm tắt: {request.DocContent}\n" +
                             $"Đơn vị ban hành: {request.IssuingUnit}\n" +
                             $"Số hiệu văn bản: {request.DocNumber ?? "N/A"}\n" +
                             $"Người ký: {request.Signer ?? "N/A"}\n";

            // Kiểm tra cấu hình Gemini API Key
            string geminiKey = _configuration["GeminiSettings:ApiKey"] ?? _configuration["GeminiApiKey"];
            
            if (!string.IsNullOrEmpty(geminiKey))
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiKey}";
                        
                        var prompt = $"Bạn là Trợ lý Pháp luật AI của CivicConnect, chuyên giải đáp thắc mắc về chính sách và tin tức tại Việt Nam.\n\n" +
                                     $"Dưới đây là thông tin văn bản người dùng đang đọc:\n{context}\n\n" +
                                     $"Người dùng hỏi: \"{request.Question}\"\n\n" +
                                     $"Hãy trả lời ngắn gọn (khoảng 3-5 câu), dễ hiểu, tập trung vào cốt lõi vấn đề và dẫn chứng cụ thể từ thông tin trên nếu có.";

                        var payload = new
                        {
                            contents = new[]
                            {
                                new { parts = new[] { new { text = prompt } } }
                            }
                        };

                        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                        var httpContent = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(requestUrl, httpContent);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            using (var doc = System.Text.Json.JsonDocument.Parse(responseBody))
                            {
                                var answer = doc.RootElement
                                    .GetProperty("candidates")[0]
                                    .GetProperty("content")
                                    .GetProperty("parts")[0]
                                    .GetProperty("text")
                                    .GetString();

                                return Json(new { success = true, answer = answer });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Gemini API Error: {ex.Message}");
                }
            }

            // Fallback: Bộ máy quy tắc trả lời pháp luật thông minh tại chỗ (Local Heuristic QA Engine)
            string questionLower = request.Question.ToLower();
            string answerText = "";

            if (questionLower.Contains("tóm tắt") || questionLower.Contains("cốt lõi") || questionLower.Contains("nội dung chính") || questionLower.Contains("gì"))
            {
                answerText = $"Nội dung cốt lõi của văn bản \"{request.DocTitle}\" xoay quanh việc: {request.DocContent}. Văn bản này do {request.IssuingUnit} ban hành.";
            }
            else if (questionLower.Contains("hiệu lực") || questionLower.Contains("khi nào") || questionLower.Contains("ngày nào"))
            {
                answerText = $"Văn bản được cập nhật ngày {request.PublishedDate:dd/MM/yyyy}. Đối với các nghị định, thông tư chính thức từ nhà nước, bạn có thể tham khảo ngày có hiệu lực và người ký cụ thể tại cột thuộc tính bên phải trang chi tiết.";
            }
            else if (questionLower.Contains("số hiệu") || questionLower.Contains("ký hiệu"))
            {
                answerText = !string.IsNullOrEmpty(request.DocNumber) 
                    ? $"Văn bản này có số hiệu chính thức là: **{request.DocNumber}**."
                    : "Đây là tin tức sự kiện đô thị hàng ngày nên không có số hiệu văn bản pháp lý chính thức.";
            }
            else if (questionLower.Contains("người ký") || questionLower.Contains("ai ký"))
            {
                answerText = !string.IsNullOrEmpty(request.Signer)
                    ? $"Văn bản này được ký bởi: **{request.Signer}**."
                    : "Đây là tin tức truyền thông đô thị nên không có thông tin người ký trực tiếp.";
            }
            else if (questionLower.Contains("mức phạt") || questionLower.Contains("phạt bao nhiêu") || questionLower.Contains("xử phạt"))
            {
                if (request.DocTitle.Contains("45/2026/NĐ-CP") || request.DocContent.Contains("xử phạt") || request.DocContent.Contains("phạt"))
                {
                    answerText = "Dựa trên Nghị định 45/2026/NĐ-CP:\n" +
                                 "- Vứt rác bừa bãi: Phạt cá nhân đến **1.000.000đ**.\n" +
                                 "- Đổ rác thải sinh hoạt sai nơi quy định (ra lòng đường, vỉa hè): Phạt cá nhân đến **5.000.000đ**.\n" +
                                 "- Các hành vi lấn chiếm vỉa hè, lòng lề đường gây mất mỹ quan: Phạt từ **10.000.000đ đến 20.000.000đ**.";
                }
                else
                {
                    answerText = "Nội dung xử phạt cụ thể chưa được chi tiết hóa trong văn bản này. Vui lòng nhấp vào Tab \"Xem trang gốc trực tiếp\" để đọc toàn văn hoặc liên hệ cơ quan tư pháp địa phương.";
                }
            }
            else
            {
                answerText = $"Chào bạn! Tôi là Trợ lý AI CivicConnect. Liên quan đến văn bản \"{request.DocTitle}\", nội dung cơ bản là: {request.DocContent}. Nếu có thắc mắc chuyên sâu, bạn có thể xem trang gốc chính thức hoặc liên hệ trực tiếp đơn vị ban hành ({request.IssuingUnit}).";
            }

            return Json(new { success = true, answer = answerText });
        }

        private async Task<List<Policy>> FetchRssNewsAsync()
        {
            var newsList = new List<Policy>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(2500);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    // Fetch live RSS feed from VnExpress
                    var xmlContent = await client.GetStringAsync("https://vnexpress.net/rss/tin-moi-nhat.rss");

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlContent);

                    var items = xmlDoc.SelectNodes("//item");
                    if (items != null)
                    {
                        int count = 0;
                        foreach (XmlNode item in items)
                        {
                            if (count >= 5) break;

                            var title = item.SelectSingleNode("title")?.InnerText ?? "";
                            var link = item.SelectSingleNode("link")?.InnerText ?? "";
                            var description = item.SelectSingleNode("description")?.InnerText ?? "";
                            var pubDateStr = item.SelectSingleNode("pubDate")?.InnerText ?? "";

                            DateTime pubDate = DateTime.UtcNow;
                            if (DateTime.TryParse(pubDateStr, out var parsedDate))
                            {
                                pubDate = parsedDate;
                            }

                            // Clean description by removing HTML tags
                            var cleanDescription = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty).Trim();

                            newsList.Add(new Policy
                            {
                                Id = 0,
                                Title = title,
                                Excerpt = cleanDescription,
                                Content = cleanDescription, // Dùng mô tả làm nội dung tóm tắt để luôn hiển thị đầy đủ
                                Tag = "Tin tức",
                                TagClass = "tag-news",
                                IssuingUnit = "Báo điện tử",
                                PublishedDate = pubDate,
                                IsActive = true,
                                SourceUrl = link
                            });

                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RSS Fetch Error: {ex.Message}");
            }
            return newsList;
        }
    }

    public class AskAIRequest
    {
        public string Question { get; set; }
        public string DocTitle { get; set; }
        public string DocContent { get; set; }
        public string IssuingUnit { get; set; }
        public string DocNumber { get; set; }
        public string Signer { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}
