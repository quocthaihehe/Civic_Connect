using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
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
        private readonly IAiService _aiService;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _scrapedContentCache = 
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

        public PolicyController(AppDbContext context, IConfiguration configuration, IAiService aiService)
        {
            _context = context;
            _configuration = configuration;
            _aiService = aiService;
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
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    // Tự động giải nén GZIP/Deflate/Brotli - fix lỗi ký tự lạ trên các trang nén nội dung
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(12);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP Error {response.StatusCode}");
                    }
                    
                    // Đọc raw bytes để tránh lỗi encoding sai (nhiều trang VN dùng Windows-1252)
                    var rawBytes = await response.Content.ReadAsByteArrayAsync();
                    var html = DecodeHtmlBytes(rawBytes, response.Content.Headers.ContentType?.CharSet);
                    
                    // Attempt to extract clean reader mode (content only)
                    var readerHtml = ExtractReaderMode(html, url);
                    if (readerHtml != null)
                    {
                        return Content(readerHtml, "text/html; charset=utf-8");
                    }
                    
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

                    var iframeScript = @"
<script>
    document.addEventListener('mouseup', function (e) {
        setTimeout(function () {
            const sel = window.getSelection();
            const text = sel ? sel.toString().trim() : '';
            if (text.length >= 5) {
                const rect = { clientX: e.clientX, clientY: e.clientY };
                if (sel.rangeCount > 0) {
                    const range = sel.getRangeAt(0);
                    const rects = range.getClientRects();
                    if (rects.length > 0) {
                        rect.clientX = rects[rects.length - 1].right;
                        rect.clientY = rects[rects.length - 1].bottom;
                    }
                }
                window.parent.postMessage({
                    type: 'iframeTextSelected',
                    text: text,
                    clientX: rect.clientX,
                    clientY: rect.clientY
                }, '*');
            }
        }, 160);
    });

    document.addEventListener('mousedown', function (e) {
        window.parent.postMessage({ type: 'iframeSelectionCleared' }, '*');
    });
</script>";

                    if (html.Contains("</body>"))
                    {
                        html = html.Replace("</body>", $"{iframeScript}\n</body>");
                    }
                    else if (html.Contains("</BODY>"))
                    {
                        html = html.Replace("</BODY>", $"{iframeScript}\n</BODY>");
                    }
                    else
                    {
                        html = html + iframeScript;
                    }
                    
                    return Content(html, "text/html; charset=utf-8");
                }
            }
            catch (Exception)
            {
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

        /// <summary>
        /// Đọc raw bytes và decode đúng encoding của trang web.
        /// Ưu tiên: 1) HTTP Content-Type header charset, 2) HTML meta charset, 3) UTF-8 mặc định.
        /// </summary>
        private static string DecodeHtmlBytes(byte[] bytes, string? headerCharset)
        {
            System.Text.Encoding encoding = null;

            // 1. Thử dùng charset từ HTTP Content-Type header
            if (!string.IsNullOrWhiteSpace(headerCharset))
            {
                try
                {
                    encoding = System.Text.Encoding.GetEncoding(headerCharset.Trim());
                }
                catch { }
            }

            // 2. Nếu không có hoặc sai, đọc tạm bằng ISO-8859-1 để tìm meta charset trong HTML
            if (encoding == null || encoding.WebName == "utf-8")
            {
                // Đọc 4KB đầu để tìm meta charset (không cần đọc hết file)
                var preview = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(bytes, 0, Math.Min(bytes.Length, 4096));

                // Tìm: <meta charset="windows-1252"> hoặc <meta http-equiv="Content-Type" content="text/html; charset=windows-1252">
                var charsetMatch = System.Text.RegularExpressions.Regex.Match(
                    preview,
                    @"charset\s*=\s*[""']?\s*([\w\-]+)\s*[""']?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (charsetMatch.Success)
                {
                    var detectedCharset = charsetMatch.Groups[1].Value.Trim();
                    try
                    {
                        var detected = System.Text.Encoding.GetEncoding(detectedCharset);
                        // Chỉ ghi đè nếu encoding tìm thấy khác UTF-8 hoặc chưa có encoding
                        if (encoding == null || detected.WebName != "utf-8")
                        {
                            encoding = detected;
                        }
                    }
                    catch { }
                }
            }

            // 3. Fallback về UTF-8
            if (encoding == null)
            {
                encoding = System.Text.Encoding.UTF8;
            }

            // Decode bytes sang string với encoding đúng
            var html = encoding.GetString(bytes);

            // Nếu encoding không phải UTF-8, cần thay charset trong meta tag thành utf-8
            // để trình duyệt không tự re-interpret sai
            if (encoding.WebName != "utf-8")
            {
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"charset\s*=\s*[""']?\s*[\w\-]+\s*[""']?",
                    "charset=utf-8",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return html;
        }

        private string? ExtractReaderMode(string html, string url)
        {
            try
            {
                // 1. Extract Title (avoiding matching the header logo <h1>)
                string title = "";
                var titleMatch = System.Text.RegularExpressions.Regex.Match(html, @"<h1[^>]*?class=""[^""]*?(detail-title|article-title)[^""]*""[^>]*?>(.*?)</h1>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (!titleMatch.Success)
                {
                    titleMatch = System.Text.RegularExpressions.Regex.Match(html, @"<h1[^>]*?class='[^']*?(detail-title|article-title)[^']*'[^>]*?>(.*?)</h1>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                }
                if (!titleMatch.Success)
                {
                    // Fallback to first non-logo h1
                    var matches = System.Text.RegularExpressions.Regex.Matches(html, @"<h1[^>]*?>(.*?)</h1>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    foreach (System.Text.RegularExpressions.Match m in matches)
                    {
                        var potentialTitle = m.Groups[1].Value;
                        if (!potentialTitle.Contains("logo") && !potentialTitle.Contains("header__logo"))
                        {
                            title = System.Text.RegularExpressions.Regex.Replace(potentialTitle, "<.*?>", "").Trim();
                            break;
                        }
                    }
                }
                else
                {
                    title = titleMatch.Groups[2].Value;
                    title = System.Text.RegularExpressions.Regex.Replace(title, "<.*?>", "").Trim();
                }

                // 2. Extract Sapo/Summary
                string sapo = "";
                var sapoRegexes = new string[]
                {
                    @"<h2[^>]*?class=""[^""]*?sapo[^""]*""[^>]*?>(.*?)</h2>",
                    @"<p[^>]*?class=""[^""]*?description[^""]*""[^>]*?>(.*?)</p>",
                    @"<div[^>]*?class=""[^""]*?sapo[^""]*""[^>]*?>(.*?)</div>",
                    @"<h2[^>]*?class='[^']*?sapo[^']*'[^>]*?>(.*?)</h2>",
                    @"<p[^>]*?class='[^']*?description[^']*'[^>]*?>(.*?)</p>",
                    @"<div[^>]*?class='[^']*?sapo[^']*'[^>]*?>(.*?)</div>"
                };

                foreach (var pattern in sapoRegexes)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(html, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (match.Success)
                    {
                        sapo = match.Groups[1].Value;
                        sapo = System.Text.RegularExpressions.Regex.Replace(sapo, "<.*?>", "").Trim();
                        break;
                    }
                }

                // Try hidden input value for hdSapo
                if (string.IsNullOrEmpty(sapo))
                {
                    var sapoInputMatch = System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*?id=""hdSapo""[^>]*?value=""(.*?)""", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (sapoInputMatch.Success)
                    {
                        sapo = sapoInputMatch.Groups[1].Value;
                        sapo = System.Net.WebUtility.HtmlDecode(sapo);
                    }
                }

                // 3. Extract main content body via robust string splitting
                string bodyHtml = "";
                int bodyStartIndex = -1;
                string[] bodyStartSelectors = new string[] { "detail-content", "fck_detail", "article-content", "main-content-body", "fck" };
                
                foreach (var selector in bodyStartSelectors)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(html, $@"<div[^>]*?class=""[^""]*?{selector}[^""]*""[^>]*?>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        bodyStartIndex = match.Index + match.Length;
                        break;
                    }
                }

                if (bodyStartIndex == -1)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(html, @"<div[^>]*?id=""(main-detail-body|article-body|content-body)""[^>]*?>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        bodyStartIndex = match.Index + match.Length;
                    }
                }

                if (bodyStartIndex != -1)
                {
                    string remainder = html.Substring(bodyStartIndex);
                    int bodyEndIndex = -1;
                    string[] endSelectors = new string[] 
                    { 
                        "detail-tab-bottom", 
                        "author-info", 
                        "social-share", 
                        "relation-news", 
                        "detail-author-bot",
                        "formreactdetail",
                        "detail-comment"
                    };

                    foreach (var selector in endSelectors)
                    {
                        int idx = remainder.IndexOf(selector, StringComparison.OrdinalIgnoreCase);
                        if (idx != -1)
                        {
                            int tagOpenIdx = remainder.LastIndexOf("<", idx);
                            if (tagOpenIdx != -1 && (bodyEndIndex == -1 || tagOpenIdx < bodyEndIndex))
                            {
                                bodyEndIndex = tagOpenIdx;
                            }
                        }
                    }

                    if (bodyEndIndex != -1)
                    {
                        bodyHtml = remainder.Substring(0, bodyEndIndex);
                    }
                    else
                    {
                        bodyHtml = remainder;
                    }
                }

                if (string.IsNullOrEmpty(bodyHtml) || bodyHtml.Length < 200)
                {
                    return null; // Trigger fallback
                }

                // 4. Clean bodyHtml (Remove comments, scripts, styles, navigation, ads, social share buttons)
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<!--.*?-->", "", System.Text.RegularExpressions.RegexOptions.Singleline);
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<script[^>]*?>.*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<style[^>]*?>.*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                
                // Remove ad/social boxes
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<div[^>]*?class=""[^""]*?(social|share|relation|ads|banner|sidebar|comment|header|footer|menu)[^""]*""[^>]*?>.*?</div>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<div[^>]*?id=""[^""]*?(social|share|relation|ads|banner|sidebar|comment|header|footer|menu)[^""]*""[^>]*?>.*?</div>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                
                // Also clean up any embedded iframe ads
                bodyHtml = System.Text.RegularExpressions.Regex.Replace(bodyHtml, @"<iframe[^>]*?>.*?</iframe>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

                var uri = new Uri(url);
                var baseHref = $"{uri.Scheme}://{uri.Host}";

                // 5. Wrap in premium Reader Mode layout
                var cleanHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <base href='{baseHref}/' />
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css'>
    <style>
        body {{
            font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background-color: #f8fafc;
            color: #1e293b;
            line-height: 1.8;
            font-size: 1.075rem;
            padding: 20px 10px;
            margin: 0;
        }}
        .reader-container {{
            max-width: 760px;
            margin: 0 auto;
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            padding: 35px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
        }}
        @media (max-width: 576px) {{
            body {{ padding: 10px 5px; }}
            .reader-container {{ padding: 20px; border-radius: 8px; }}
        }}
        .article-title {{
            font-size: 1.85rem;
            font-weight: 800;
            color: #0f172a;
            line-height: 1.35;
            margin-bottom: 12px;
        }}
        .article-meta {{
            font-size: 0.825rem;
            color: #64748b;
            margin-bottom: 20px;
            border-bottom: 1px solid #e2e8f0;
            padding-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        .sapo {{
            font-size: 1.125rem;
            font-weight: 600;
            color: #334155;
            line-height: 1.6;
            margin-bottom: 20px;
            background-color: #f1f5f9;
            border-left: 4px solid #3b82f6;
            padding: 12px 16px;
            border-radius: 0 8px 8px 0;
        }}
        .content-body {{
            color: #334155;
        }}
        .content-body p {{
            margin-bottom: 16px;
        }}
        .content-body img {{
            max-width: 100%;
            height: auto;
            border-radius: 10px;
            margin: 16px 0;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }}
        .content-body figure {{
            margin: 20px 0;
            text-align: center;
        }}
        .content-body figcaption {{
            font-size: 0.825rem;
            color: #64748b;
            margin-top: 6px;
            font-style: italic;
        }}
        .source-footer {{
            margin-top: 30px;
            padding-top: 15px;
            border-top: 1px dashed #e2e8f0;
            font-size: 0.825rem;
            color: #94a3b8;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .btn-view-original {{
            text-decoration: none;
            color: #3b82f6;
            font-weight: 600;
            display: inline-flex;
            align-items: center;
            gap: 4px;
        }}
        .btn-view-original:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class='reader-container'>
        {(string.IsNullOrEmpty(title) ? "" : $"<h1 class='article-title'>{title}</h1>")}
        <div class='article-meta'>
            <span><i class='bi bi-newspaper'></i> Báo chí chính thống</span>
            <span>•</span>
            <span><i class='bi bi-patch-check-fill' style='color:#0d6efd;'></i> Đã kiểm chứng</span>
        </div>
        {(string.IsNullOrEmpty(sapo) ? "" : $"<div class='sapo'>{sapo}</div>")}
        <div class='content-body'>
            {bodyHtml}
        </div>
        <div class='source-footer'>
            <span>Nguồn: {new Uri(url).Host}</span>
            <a href='{url}' target='_blank' class='btn-view-original'>Xem bài viết gốc <i class='bi bi-box-arrow-up-right'></i></a>
        </div>
    </div>
    <script>
        document.addEventListener('mouseup', function (e) {{
            setTimeout(function () {{
                const sel = window.getSelection();
                const text = sel ? sel.toString().trim() : '';
                if (text.length >= 5) {{
                    const rect = {{ clientX: e.clientX, clientY: e.clientY }};
                    if (sel.rangeCount > 0) {{
                        const range = sel.getRangeAt(0);
                        const rects = range.getClientRects();
                        if (rects.length > 0) {{
                            rect.clientX = rects[rects.length - 1].right;
                            rect.clientY = rects[rects.length - 1].bottom;
                        }}
                    }}
                    window.parent.postMessage({{
                        type: 'iframeTextSelected',
                        text: text,
                        clientX: rect.clientX,
                        clientY: rect.clientY
                    }}, '*');
                }}
            }}, 160);
        }});

        document.addEventListener('mousedown', function (e) {{
            window.parent.postMessage({{ type: 'iframeSelectionCleared' }}, '*');
        }});
    </script>
</body>
</html>";
                return cleanHtml;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reader Mode extraction failed: {ex.Message}");
                return null;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AskAI([FromBody] AskAIRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Question))
            {
                return Json(new { success = false, answer = "Câu hỏi hoặc dữ liệu không hợp lệ." });
            }

            // Mặc định sử dụng tóm tắt truyền từ client
            string fullTextContext = request.DocContent;

            // Nếu đây là tin tức ngoài có URL, tự động cào toàn văn bài viết để làm ngữ cảnh
            if (!string.IsNullOrEmpty(request.SourceUrl) && request.SourceUrl.StartsWith("http"))
            {
                var scrapedText = await ScrapeUrlTextAsync(request.SourceUrl);
                if (!string.IsNullOrEmpty(scrapedText))
                {
                    fullTextContext = scrapedText;
                }
            }

            string context = $"Tên văn bản/Chính sách/Tin tức: {request.DocTitle}\n" +
                             $"Nội dung chi tiết/Toàn văn: {fullTextContext}\n" +
                             $"Đơn vị ban hành/Nguồn tin: {request.IssuingUnit}\n" +
                             $"Số hiệu văn bản: {request.DocNumber ?? "N/A"}\n" +
                             $"Người ký: {request.Signer ?? "N/A"}\n";

            // Kiểm tra cấu hình Gemini API Key từ nhiều nguồn để tăng độ ổn định
            string geminiKey = _configuration["GeminiSettings:ApiKey"] 
                ?? _configuration["GeminiApiKey"] 
                ?? Environment.GetEnvironmentVariable("GeminiApiKey")
                ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            
            if (!string.IsNullOrEmpty(geminiKey))
            {
                geminiKey = geminiKey.Trim(' ', '"', '\'', '\n', '\r');
                
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(12);
                        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiKey}";
                        
                        var prompt = $"Bạn là Trợ lý Pháp luật và Tin tức AI của CivicConnect, chuyên giải đáp thắc mắc về chính sách và tin tức tại Việt Nam.\n\n" +
                                     $"Dưới đây là TOÀN BỘ nội dung chi tiết bài viết/văn bản người dùng đang đọc:\n{context}\n\n" +
                                     $"Người dùng hỏi: \"{request.Question}\"\n\n" +
                                     $"Yêu cầu trả lời:\n" +
                                     $"- Phân tích và giải đáp thật chi tiết, đầy đủ thông tin, rõ ràng mạch lạc.\n" +
                                     $"- Nắm bắt toàn bộ nội dung của tin tức/chính sách để giúp người dùng hiểu cặn kẽ 90% đến 100% cốt lõi của vấn đề.\n" +
                                     $"- Dẫn chứng cụ thể số liệu, địa danh, tên người, mốc thời gian hoặc điều luật có trong nội dung chi tiết trên.\n" +
                                     $"- Trình bày chuyên nghiệp, sử dụng định dạng danh sách (bullet points) hoặc in đậm (**chữ đậm**) để dễ đọc và làm nổi bật các điểm quan trọng.";

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
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            using (var doc = System.Text.Json.JsonDocument.Parse(responseBody))
                            {
                                var root = doc.RootElement;
                                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                                {
                                    var firstCandidate = candidates[0];
                                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                                        contentObj.TryGetProperty("parts", out var parts) &&
                                        parts.GetArrayLength() > 0)
                                    {
                                        var answer = parts[0].GetProperty("text").GetString();
                                        return Json(new { success = true, answer = answer });
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Lỗi gọi API sẽ tự động nhảy xuống Fallback cục bộ
                }
            }

            // Fallback: Tìm kiếm trong Bộ Cơ sở Dữ liệu Pháp luật Cục bộ và Toàn văn bài viết
            string answerText = SearchLocalKnowledge(request.Question, request.DocTitle, fullTextContext, request.PublishedDate, geminiKey);
            return Json(new { success = true, answer = answerText });
        }

        private static string SearchLocalKnowledge(string question, string docTitle, string docContent, DateTime publishedDate, string geminiKey)
        {
            string questionLower = question.ToLower();
            
            // 1. Tìm kiếm trong Dataset Luật & Chính sách cục bộ
            LawEntry bestMatchLaw = null;
            int highestLawScore = 0;
            
            foreach (var law in LawDataset)
            {
                int score = 0;
                var keywords = law.Keywords.Split(',');
                foreach (var kw in keywords)
                {
                    var trimmedKw = kw.Trim().ToLower();
                    if (string.IsNullOrEmpty(trimmedKw)) continue;
                    
                    if (questionLower.Contains(trimmedKw))
                    {
                        score += 3;
                    }
                }
                
                if (score > highestLawScore)
                {
                    highestLawScore = score;
                    bestMatchLaw = law;
                }
            }
            
            // 2. Tìm kiếm các câu chứa từ khóa trong Toàn văn bài viết hiện tại
            var docSentences = docContent.Split(new[] { '.', '?', '!', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 8)
                .ToList();
                
            var matchedSentences = new List<string>();
            int highestSentenceScore = 0;
            
            // Lọc các từ vô nghĩa khi so khớp câu
            var stopWords = new HashSet<string> { "là", "bị", "phạt", "bao", "nhiêu", "của", "và", "trong", "có", "không", "cho", "để", "làm", "gì", "thế", "nào" };
            var questionWords = questionLower.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .ToList();

            foreach (var sentence in docSentences)
            {
                int sentenceScore = 0;
                foreach (var qw in questionWords)
                {
                    if (sentence.ToLower().Contains(qw))
                    {
                        sentenceScore += 2;
                    }
                }
                
                if (sentenceScore > 2)
                {
                    matchedSentences.Add(sentence);
                    if (sentenceScore > highestSentenceScore)
                    {
                        highestSentenceScore = sentenceScore;
                    }
                }
            }
            
            // 3. Tổng hợp câu trả lời
            string response = "";
            
            // Nếu có kết quả đối khớp luật
            if (bestMatchLaw != null && highestLawScore >= 3)
            {
                response += $"### 📜 Kết quả Tra cứu Cơ sở Dữ liệu Pháp luật:\n" +
                            $"**Bộ luật/Nghị định:** {bestMatchLaw.Title}\n" +
                            $"- **Nội dung quy định:** {bestMatchLaw.Content}\n" +
                            $"- **Mức xử phạt hành chính:** **{bestMatchLaw.PenaltyInfo}**\n";
                            
                if (!string.IsNullOrEmpty(bestMatchLaw.DocumentNumber))
                {
                    response += $"- **Số hiệu văn bản pháp lý:** `{bestMatchLaw.DocumentNumber}`\n";
                }
                response += "\n";
            }
            
            // Nếu tìm thấy các câu trả lời trực tiếp trong bài viết/tài liệu hiện tại
            if (matchedSentences.Any())
            {
                response += $"### 📰 Trích dẫn thông tin từ bài viết hiện tại:\n";
                var uniqueSentences = matchedSentences.Distinct().Take(3);
                foreach (var sent in uniqueSentences)
                {
                    response += $"- *\"{sent}.\"*\n";
                }
                response += "\n";
            }
            
            // Nếu không khớp được gì cụ thể, cung cấp tóm tắt văn bản tổng quát
            if (string.IsNullOrEmpty(response))
            {
                response = $"### 🤖 Trợ lý CivicConnect (Chế độ Offline):\n" +
                           $"Tôi đã nhận được câu hỏi: *\"{question}\"*.\n\n" +
                           $"Hiện tại cơ sở dữ liệu luật cục bộ chưa khớp từ khóa cụ thể. Dưới đây là thông tin tóm tắt cốt lõi của tài liệu này để bạn tham khảo:\n" +
                           $"- **Tên tài liệu/Tin tức:** {docTitle}\n" +
                           $"- **Nội dung cốt lõi:** {docContent}\n" +
                           $"- **Đăng tải vào lúc:** {publishedDate:dd/MM/yyyy HH:mm}\n\n" +
                           $"*Gợi ý:* Bạn hãy thử hỏi về các chủ đề luật xử phạt như: *vứt rác bừa bãi, lấn chiếm vỉa hè, nồng độ cồn rượu bia, không đội mũ bảo hiểm, karaoke làm ồn ban đêm, xây nhà không phép, chó thả rông không rọ mõm, hoặc đốt pháo tết* để tôi có thể tra cứu luật chính xác nhất!";
            }
            
            // Đính kèm ghi chú chân trang cấu hình
            if (string.IsNullOrEmpty(geminiKey))
            {
                response += $"\n---\n*💡 Lưu ý: Trợ lý đang hoạt động ở chế độ **Offline cục bộ** (Chưa cấu hình Gemini API Key). Để mở rộng khả năng suy luận vô hạn và giải đáp tự do, vui lòng mở file `appsettings.json` và điền khóa của bạn vào trường `\"GeminiApiKey\"`.*";
            }
            else
            {
                response += $"\n---\n*💡 Lưu ý: Hệ thống phát hiện cuộc gọi đến API Gemini thất bại (có thể khóa hết hạn mức hoặc không hợp lệ). Trợ lý đã chuyển sang **phản hồi từ cơ sở dữ liệu pháp luật ngoại tuyến** để hỗ trợ bạn.*";
            }
            
            return response;
        }

        private async Task<string> ScrapeUrlTextAsync(string url)
        {
            if (_scrapedContentCache.TryGetValue(url, out var cachedText))
            {
                return cachedText;
            }

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    // Tự động giải nén GZIP/Deflate - fix lỗi ký tự lạ khi scrape
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(12);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return null;
                    
                    // Đọc raw bytes để tránh lỗi encoding (giống Proxy method)
                    var rawBytes = await response.Content.ReadAsByteArrayAsync();
                    var html = DecodeHtmlBytes(rawBytes, response.Content.Headers.ContentType?.CharSet);
                    
                    // Xóa các thẻ script và style
                    html = System.Text.RegularExpressions.Regex.Replace(html, "<script[^>]*?>.*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    html = System.Text.RegularExpressions.Regex.Replace(html, "<style[^>]*?>.*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    
                    // Loại bỏ tất cả thẻ HTML để lấy văn bản thuần
                    var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
                    plainText = System.Net.WebUtility.HtmlDecode(plainText);
                    
                    // Chuẩn hóa khoảng trắng
                    plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();
                    
                    // Giới hạn độ dài ngữ cảnh để tránh tràn token (khoảng 8000 ký tự)
                    if (plainText.Length > 8000)
                    {
                        plainText = plainText.Substring(0, 8000);
                    }
                    
                    _scrapedContentCache[url] = plainText;
                    return plainText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scraping failed: {ex.Message}");
                return null;
            }
        }

        private async Task<List<Policy>> FetchRssNewsAsync()
        {
            var newsList = new List<Policy>();
            
            // Define RSS sources: (url, isGovSource, sourceName)
            var rssSources = new[]
            {
                // ===== Nguồn chính thống từ Báo điện tử Chính phủ (baochinhphu.vn) =====
                new { Url = "https://baochinhphu.vn/rss", IsGov = true, Name = "Báo Chính phủ", Tag = "Chính phủ", TagClass = "tag-news" },
                // ===== Nguồn báo điện tử =====
                new { Url = "https://vnexpress.net/rss/tin-moi-nhat.rss", IsGov = false, Name = "VnExpress", Tag = "Tin tức", TagClass = "tag-news" },
            };

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(3500);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                foreach (var source in rssSources)
                {
                    try
                    {
                        var xmlContent = await client.GetStringAsync(source.Url);
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlContent);

                        var items = xmlDoc.SelectNodes("//item");
                        if (items == null) continue;

                        int maxPerSource = source.IsGov ? 12 : 5;
                        int count = 0;

                        foreach (XmlNode item in items)
                        {
                            if (count >= maxPerSource) break;

                            var title = item.SelectSingleNode("title")?.InnerText ?? "";
                            var link = item.SelectSingleNode("link")?.InnerText ?? "";
                            var description = item.SelectSingleNode("description")?.InnerText ?? "";
                            var pubDateStr = item.SelectSingleNode("pubDate")?.InnerText ?? "";

                            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                            DateTime pubDate = DateTime.UtcNow;
                            if (DateTime.TryParse(pubDateStr, out var parsedDate))
                            {
                                pubDate = parsedDate;
                            }

                            // Clean description HTML
                            var cleanDescription = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty).Trim();
                            if (cleanDescription.Length > 300) cleanDescription = cleanDescription.Substring(0, 300) + "...";

                            newsList.Add(new Policy
                            {
                                Id = 0,
                                Title = title,
                                Excerpt = cleanDescription,
                                Content = cleanDescription,
                                Tag = source.Tag,
                                TagClass = source.TagClass,
                                IssuingUnit = source.Name,
                                PublishedDate = pubDate,
                                IsActive = true,
                                SourceUrl = link
                            });

                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"RSS Fetch Error [{source.Name}]: {ex.Message}");
                    }
                }
            }
            return newsList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summarize(int id)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(id);
                if (policy == null || !policy.IsActive)
                    return Json(new { success = false, error = "Không tìm thấy chính sách." });

                var existingSummary = await _context.PolicyAiSummaries
                    .Where(s => s.PolicyId == id && s.IsActive)
                    .OrderByDescending(s => s.GeneratedAt)
                    .FirstOrDefaultAsync();

                if (existingSummary != null)
                {
                    var cachedBullets = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                        existingSummary.BulletPointsJson) ?? new List<string>();

                    return Json(new
                    {
                        success = true,
                        shortSummary = existingSummary.ShortSummary,
                        bulletPoints = cachedBullets,
                        realWorldExample = existingSummary.RealWorldExample,
                        generatedAt = existingSummary.GeneratedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                        modelUsed = existingSummary.AiModel,
                        fromCache = true
                    });
                }

                var contentToSummarize = string.IsNullOrWhiteSpace(policy.Content) || policy.Content.Length < 50
                    ? policy.Excerpt
                    : policy.Content;

                var result = await _aiService.SummarizePolicyAsync(policy.Title, contentToSummarize);

                if (!result.IsSuccess)
                {
                    return Json(new
                    {
                        success = false,
                        error = result.ErrorMessage ?? "Không thể tóm tắt lúc này. Vui lòng thử lại sau."
                    });
                }

                var summary = new PolicyAiSummary
                {
                    PolicyId = id,
                    ShortSummary = result.ShortSummary,
                    BulletPointsJson = System.Text.Json.JsonSerializer.Serialize(result.BulletPoints),
                    RealWorldExample = result.RealWorldExample,
                    AiModel = result.ModelUsed,
                    TokensUsed = result.TokensUsed,
                    GeneratedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.PolicyAiSummaries.Add(summary);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    shortSummary = result.ShortSummary,
                    bulletPoints = result.BulletPoints,
                    realWorldExample = result.RealWorldExample,
                    generatedAt = summary.GeneratedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    modelUsed = result.ModelUsed,
                    fromCache = false
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    error = "Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại sau."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExplainSelection([FromBody] ExplainSelectionRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.SelectedText) ||
                request.SelectedText.Trim().Length < 5)
            {
                return Json(new { success = false, error = "Vui lòng bôi đen một đoạn văn bản để giải thích." });
            }

            try
            {
                var policy = await _context.Policies.FindAsync(request.PolicyId);
                var policyTitle = policy?.Title ?? "Chính sách";

                var result = await _aiService.ExplainSelectionAsync(
                    request.SelectedText.Trim(), policyTitle);

                if (!result.IsSuccess)
                    return Json(new { success = false, error = result.ErrorMessage ?? "Không thể giải thích lúc này. Vui lòng thử lại." });

                return Json(new
                {
                    success = true,
                    simpleExplanation = result.SimpleExplanation,
                    keyTerm = result.KeyTerm,
                    practicalMeaning = result.PracticalMeaning,
                    modelUsed = result.ModelUsed
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadAndExplain(int id)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(id);
                if (policy == null || !policy.IsActive)
                    return Json(new { success = false, error = "Không tìm thấy chính sách." });

                var contentToRead = string.IsNullOrWhiteSpace(policy.Content) || policy.Content.Length < 50
                    ? policy.Excerpt
                    : policy.Content;

                var result = await _aiService.ReadAndExplainAsync(policy.Title, contentToRead);

                if (!result.IsSuccess)
                    return Json(new { success = false, error = result.ErrorMessage ?? "Không thể phân tích lúc này. Vui lòng thử lại sau." });

                return Json(new
                {
                    success = true,
                    overallGist = result.OverallGist,
                    modelUsed = result.ModelUsed,
                    sections = result.Sections.Select(s => new
                    {
                        title = s.Title,
                        originalText = s.OriginalText,
                        simpleExplanation = s.SimpleExplanation,
                        actionRequired = s.ActionRequired
                    }).ToList()
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }

        private static readonly List<LawEntry> LawDataset = new List<LawEntry>
        {
            new LawEntry
            {
                Title = "Xử phạt vi phạm vệ sinh môi trường đô thị",
                Keywords = "rác, vứt rác, xả rác, vệ sinh, môi trường, đổ rác, sinh hoạt, ngõ hẻm, bừa bãi",
                Content = "Quy định xử phạt đối với các hành vi vứt, thải, bỏ rác thải sinh hoạt sai nơi quy định tại chung cư, trung tâm thương mại hoặc nơi công cộng đô thị.",
                DocumentNumber = "45/2026/NĐ-CP (Nghị định Chính phủ)",
                PenaltyInfo = "Phạt tiền từ 500.000đ đến 1.000.000đ đối với hành vi vứt rác tại chung cư, nơi công cộng. Phạt tiền từ 1.000.000đ đến 2.000.000đ đối với hành vi vứt rác bừa bãi ra vỉa hè, lòng đường hoặc vào hệ thống thoát nước đô thị."
            },
            new LawEntry
            {
                Title = "Xử phạt hành vi lấn chiếm vỉa hè, lòng lề đường",
                Keywords = "vỉa hè, lòng đường, lấn chiếm, buôn bán, đỗ xe, hè phố, đậu xe, đỗ trái phép, chiếm dụng",
                Content = "Xử phạt các tổ chức, cá nhân tự ý sử dụng trái phép lòng đường đô thị hoặc hè phố để làm nơi bày bán hàng hóa, kinh doanh dịch vụ ăn uống, đặt biển hiệu quảng cáo hoặc dừng đỗ xe cản trở giao thông.",
                DocumentNumber = "Nghị định 100/2019/NĐ-CP",
                PenaltyInfo = "Phạt tiền từ 2.000.000đ đến 3.000.000đ đối với cá nhân (4.000.000đ - 6.000.000đ đối với tổ chức) bày bán hàng hóa lấn chiếm hè phố. Phạt từ 6.000.000đ đến 8.000.000đ đối với hành vi dựng rạp, lấn chiếm lòng lề đường trái phép."
            },
            new LawEntry
            {
                Title = "Quy định bắt buộc đội mũ bảo hiểm khi đi xe máy",
                Keywords = "mũ bảo hiểm, không đội mũ, quai mũ, xe máy, mô tô, xe gắn máy, đi xe",
                Content = "Người điều khiển, người ngồi trên xe mô tô hai bánh, xe mô tô ba bánh, xe gắn máy phải đội mũ bảo hiểm cho người đi mô tô, xe máy và cài quai đúng quy cách khi tham gia giao thông đường bộ.",
                DocumentNumber = "Nghị định 100/2019/NĐ-CP (sửa đổi bởi Nghị định 123/2021/NĐ-CP)",
                PenaltyInfo = "Phạt tiền từ 400.000đ đến 600.000đ đối với người điều khiển hoặc người ngồi sau không đội mũ bảo hiểm hoặc đội mũ bảo hiểm không cài quai đúng quy cách."
            },
            new LawEntry
            {
                Title = "Xử phạt vi phạm nồng độ cồn khi lái xe",
                Keywords = "nồng độ cồn, cồn, rượu, bia, say xỉn, thổi cồn, đo cồn, uống rượu, uống bia, uống rượu bia",
                Content = "Cấm tuyệt đối người điều khiển phương tiện giao thông (ô tô, xe máy, xe đạp, máy kéo) tham gia giao thông khi trong máu hoặc hơi thở có nồng độ cồn.",
                DocumentNumber = "Nghị định 100/2019/NĐ-CP",
                PenaltyInfo = "Đối với xe máy: Phạt tiền từ 2-3 triệu đồng (nồng độ cồn chưa vượt quá 50mg/100ml máu); 4-5 triệu đồng (vượt quá 50-80mg); 6-8 triệu đồng (vượt quá 80mg) kèm tước GPLX từ 10 - 24 tháng. Đối với ô tô: Phạt tối đa từ 30 - 40 triệu đồng và tước GPLX 22 - 24 tháng đối với mức vi phạm cao nhất."
            },
            new LawEntry
            {
                Title = "Gây tiếng ồn lớn trong khu dân cư (Hát Karaoke loa kéo)",
                Keywords = "tiếng ồn, ồn, karaoke, loa kéo, làm ồn, nhạc to, ban đêm, hát karaoke, huyên náo",
                Content = "Quy định xử phạt hành vi gây tiếng động lớn, làm ồn ào, huyên náo tại khu dân cư, nơi công cộng trong khoảng thời gian từ 22 giờ ngày hôm trước đến 6 giờ sáng ngày hôm sau.",
                DocumentNumber = "Nghị định 144/2021/NĐ-CP",
                PenaltyInfo = "Phạt cảnh cáo hoặc phạt tiền từ 500.000đ đến 1.000.000đ đối với cá nhân gây tiếng ồn lớn sau 22 giờ đêm. Nếu tiếng ồn vượt quy chuẩn kỹ thuật môi trường do các cơ sở kinh doanh phát ra, mức phạt từ 1.000.000đ lên tới 160.000.000đ."
            },
            new LawEntry
            {
                Title = "Xây dựng nhà ở không phép, sửa chữa trái phép",
                Keywords = "xây dựng, xây nhà, không phép, sửa nhà, trái phép, thi công, công trình, xây dựng trái phép",
                Content = "Xử phạt hành vi tổ chức thi công xây dựng công trình không có giấy phép xây dựng mà theo quy định phải có giấy phép xây dựng.",
                DocumentNumber = "Nghị định 16/2022/NĐ-CP",
                PenaltyInfo = "Phạt tiền từ 60.000.000đ đến 80.000.000đ đối với xây dựng nhà ở riêng lẻ không có giấy phép xây dựng. Biện pháp khắc phục là buộc dừng thi công, xin cấp phép trong thời hạn quy định hoặc buộc phá dỡ phần công trình vi phạm."
            },
            new LawEntry
            {
                Title = "Quy định về nuôi chó, thả rông và rọ mõm vật nuôi",
                Keywords = "chó, thả rông, rọ mõm, vật nuôi, xích chó, chó cắn, thú cưng, chó thả rông",
                Content = "Chủ nuôi chó phải đăng ký, tiêm phòng dại định kỳ và bắt buộc phải đeo rọ mõm cho chó, xích giữ hoặc có người dắt khi đưa chó ra nơi công cộng nhằm bảo đảm an toàn cho người xung quanh.",
                DocumentNumber = "Nghị định 90/2017/NĐ-CP (sửa đổi bởi Nghị định 04/2020/NĐ-CP)",
                PenaltyInfo = "Phạt tiền từ 1.000.000đ đến 2.000.000đ đối với hành vi không đeo rọ mõm cho chó hoặc không xích giữ chó khi đưa ra nơi công cộng. Buộc bồi thường thiệt hại nếu chó cắn người khác gây thương tích."
            },
            new LawEntry
            {
                Title = "Quy định cấm đốt pháo nổ, pháo hoa nổ trái phép",
                Keywords = "pháo, đốt pháo, pháo hoa, pháo nổ, tết, chất nổ, tàng trữ pháo, pháo hoa nổ",
                Content = "Nghiêm cấm hành vi sử dụng các loại pháo nổ, pháo hoa nổ trái pháp luật. Người dân chỉ được phép sử dụng pháo hoa không nổ (chỉ phát sáng) do Bộ Quốc phòng sản xuất vào các dịp lễ tết.",
                DocumentNumber = "Nghị định 144/2021/NĐ-CP",
                PenaltyInfo = "Phạt tiền từ 1.000.000đ đến 2.000.000đ đối với hành vi sử dụng các loại pháo hoa nổ, pháo nổ trái phép. Phạt từ 5.000.000đ đến 10.000.000đ đối với hành vi chế tạo, tàng trữ hoặc vận chuyển pháo trái phép."
            },
            new LawEntry
            {
                Title = "Bảo vệ dòng kênh Nhiêu Lộc - Thị Nghè",
                Keywords = "nhiêu lộc, thị nghè, câu cá, kênh, vớt rác, dòng sông, rác thải, dòng kênh",
                Content = "Quy định cấm tuyệt đối các hành vi xả rác thải sinh hoạt xuống lòng kênh Nhiêu Lộc - Thị Nghè, câu cá giải trí trái phép hoặc làm tổn hại mỹ quan, cảnh quan dọc tuyến kênh xanh.",
                DocumentNumber = "Quy chế Quản lý đô thị TP.HCM",
                PenaltyInfo = "Hành vi câu cá trộm dọc kênh Nhiêu Lộc bị xử phạt từ 1.000.000đ đến 2.000.000đ. Hành vi vứt rác sinh hoạt xuống sông, kênh rạch bị phạt tiền từ 1.000.000đ đến 2.000.000đ."
            },
            new LawEntry
            {
                Title = "Quyền phản ánh, kiến nghị trật tự đô thị của người dân",
                Keywords = "phản ánh, kiến nghị, gửi phản ánh, tố giác, khiếu nại, dân bàn, ý kiến",
                Content = "Người dân có quyền và trách nhiệm gửi phản ánh, kiến nghị về các hành vi vi phạm trật tự công cộng, lấn chiếm vỉa hè hoặc xả rác bừa bãi lên chính quyền địa phương qua ứng dụng đô thị thông minh để kịp thời xử lý.",
                DocumentNumber = "Luật Thực hiện dân chủ ở cơ sở 2022",
                PenaltyInfo = "Chính quyền cấp Phường/Quận có nghĩa vụ tiếp nhận, phản hồi và công khai tiến độ xử lý đơn thư phản ánh của cư dân trong thời hạn từ 3 - 5 ngày làm việc."
            },
            new LawEntry
            {
                Title = "Bồi thường và tái định cư khi Nhà nước thu hồi đất",
                Keywords = "đất đai, thu hồi đất, bồi thường đất, tái định cư, giá đất, bồi thường thu hồi, thu hồi",
                Content = "Quy định nguyên tắc bồi thường về đất khi Nhà nước thu hồi đất. Việc bồi thường được thực hiện bằng cách giao đất cùng mục đích sử dụng hoặc bồi thường bằng tiền theo giá đất cụ thể. Người dân bị thu hồi đất ở phải được bố trí nhà ở hoặc đất ở tái định cư, đảm bảo chỗ ở và ổn định cuộc sống trước khi phê duyệt phương án.",
                DocumentNumber = "Luật Đất đai 2024 (Luật số 31/2024/QH15)",
                PenaltyInfo = "Ủy ban nhân dân cấp có thẩm quyền có nghĩa vụ hoàn thiện khu tái định cư (hạ tầng kỹ thuật, hạ tầng xã hội đồng bộ) trước khi phê duyệt phương án bồi thường, hỗ trợ, tái định cư."
            }
        };
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
        public string SourceUrl { get; set; }
    }

    public class LawEntry
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Content { get; set; }
        public string DocumentNumber { get; set; }
        public string PenaltyInfo { get; set; }
    }

    public class ExplainSelectionRequest
    {
        public int PolicyId { get; set; }
        public string SelectedText { get; set; } = string.Empty;
    }
}
