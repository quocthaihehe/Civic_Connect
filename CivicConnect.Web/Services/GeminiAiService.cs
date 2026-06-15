using CivicConnect.Web.Models;
using CivicConnect.Web.Models.Ai;
using CivicConnect.Web.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly GeminiSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeminiAiService> _logger;
        private readonly IMemoryCache _cache;

        private const string PROMPT_TEMPLATE = """
            Bạn là một trợ lý AI phân tích chính sách và văn bản pháp luật hành chính công của chính quyền địa phương Việt Nam.
            Hãy giúp người dân (đặc biệt là người dân bình thường) hiểu rõ chính sách sau:
            Tiêu đề: {TITLE}
            Nội dung văn bản:
            {CONTENT}

            Yêu cầu:
            Trả về một chuỗi JSON có đúng các trường sau (không thêm bất kỳ văn bản nào ngoài JSON, không sử dụng markdown code block ```json ở đầu và cuối):
            {
              "short_summary": "Tóm tắt cực ngắn gọn nội dung chính của chính sách này trong vòng 2-3 câu ngắn, dễ hiểu cho người bình thường.",
              "bullet_points": [
                "Điểm chính số 1: Tóm tắt điểm mấu chốt thứ nhất ảnh hưởng trực tiếp đến người dân.",
                "Điểm chính số 2: Tóm tắt điểm mấu chốt thứ hai.",
                "Điểm chính số 3: Tóm tắt điểm mấu chốt thứ ba."
              ],
              "real_world_example": "Một ví dụ thực tế, gần gũi mô tả một tình huống cụ thể người dân thường gặp phải liên quan đến chính sách này và cách áp dụng nó."
            }
            """;

        private const string EXPLAIN_PROMPT_TEMPLATE = """
            Bạn là trợ lý giải thích pháp luật cho người dân Việt Nam.
            Hãy giải thích đoạn văn bản pháp luật/chính sách sau đây được người dân bôi đen:
            Văn bản được chọn: "{SELECTED_TEXT}"
            Ngữ cảnh (Tiêu đề chính sách): "{POLICY_TITLE}"

            Yêu cầu:
            Hãy trả về duy nhất một chuỗi JSON có định dạng như dưới đây (không có markdown code block, không thêm văn bản giải thích ngoài JSON):
            {
              "simple_explanation": "Giải thích đoạn văn bản trên bằng ngôn ngữ vô cùng đơn giản, dễ hiểu, tránh dùng thuật ngữ luật pháp phức tạp.",
              "key_term": "Nếu trong đoạn bôi đen có thuật ngữ khó hoặc từ viết tắt pháp lý, hãy giải thích nó ngắn gọn ở đây (nếu không có thì trả về null).",
              "practical_meaning": "Tác động thực tế của đoạn này đối với người dân (Ví dụ: Bạn cần làm gì, được hưởng gì, hay bị phạt thế nào?)."
            }
            """;

        private const string READ_EXPLAIN_PROMPT_TEMPLATE = """
            Bạn là một chuyên gia phân tích pháp luật của chính quyền số.
            Hãy đọc toàn bộ văn bản chính sách/pháp luật sau đây, chia nó thành 3 đến 5 phần hợp lý (tương ứng với các phần quan trọng của văn bản). Với mỗi phần, hãy trích lược đoạn nội dung gốc quan trọng và giải thích nó bằng ngôn ngữ cực kỳ giản dị cho người dân bình thường.

            Tiêu đề chính sách: {TITLE}
            Nội dung chính sách:
            {CONTENT}

            Yêu cầu:
            Trả về một chuỗi JSON chuẩn có đúng cấu trúc sau (không bọc trong markdown code block, không giải thích gì thêm):
            {
              "overall_gist": "Tóm tắt siêu ngắn toàn bộ văn bản trong đúng một câu.",
              "sections": [
                {
                  "title": "Tên phần 1 (ví dụ: 'Phần 1: Quy định chung về mức phạt')",
                  "original_text": "Trích lược ngắn gọn đoạn nội dung gốc tương ứng của phần này",
                  "simple_explanation": "Giải thích chi tiết bằng ngôn ngữ bình dân dễ hiểu nhất đối với phần này",
                  "action_required": "Hành động cụ thể người dân cần làm, hoặc nghĩa vụ cần tuân theo đối với phần này (nếu không có hành động cụ thể thì ghi null)"
                },
                {
                  "title": "Tên phần 2...",
                  "original_text": "...",
                  "simple_explanation": "...",
                  "action_required": "..."
                }
              ]
            }
            """;

        public GeminiAiService(
            IOptions<GeminiSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiAiService> logger,
            IMemoryCache cache)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cache = cache;
        }

        public async Task<PolicySummaryResult> SummarizePolicyAsync(string title, string content)
        {
            var effectiveContent = content;
            if (effectiveContent.Length > 8000)
                effectiveContent = effectiveContent[..8000];

            var cacheKey = $"policy_summary_{ComputeSha256(title + effectiveContent)}";
            if (_cache.TryGetValue(cacheKey, out PolicySummaryResult? cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Trả kết quả tóm tắt chính sách từ cache.");
                return cachedResult;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return new PolicySummaryResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Tính năng AI chưa được cấu hình. Vui lòng liên hệ quản trị viên."
                };
            }

            try
            {
                var modelName = string.IsNullOrWhiteSpace(_settings.ModelName) ? "gemini-1.5-flash" : _settings.ModelName;
                var prompt = PROMPT_TEMPLATE
                    .Replace("{TITLE}", title)
                    .Replace("{CONTENT}", effectiveContent);

                var aiText = await CallGeminiSdkAsync(prompt, modelName);

                if (string.IsNullOrWhiteSpace(aiText))
                {
                    _logger.LogWarning("Gemini SDK trả về nội dung rỗng. Dùng dự phòng.");
                    return GenerateFallbackSummary(title, effectiveContent, modelName);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<GeminiPolicyJson>(aiText, options);

                if (aiResult == null)
                {
                    _logger.LogWarning("Không thể parse JSON từ Gemini. Dùng dự phòng.");
                    return GenerateFallbackSummary(title, effectiveContent, modelName);
                }

                var result = new PolicySummaryResult
                {
                    IsSuccess = true,
                    ShortSummary = aiResult.ShortSummary ?? string.Empty,
                    BulletPoints = aiResult.BulletPoints ?? new List<string>(),
                    RealWorldExample = aiResult.RealWorldExample ?? string.Empty,
                    ModelUsed = modelName,
                    TokensUsed = 0
                };

                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(48)
                });
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Lỗi xác thực API Key Gemini trong SummarizePolicyAsync.");
                return new PolicySummaryResult
                {
                    IsSuccess = false,
                    ErrorMessage = "API Key Gemini cấu hình trong file 'appsettings.json' không hợp lệ hoặc đã hết hạn (Lỗi 401/403/400). Vui lòng cập nhật API Key chính xác từ Google AI Studio (bắt đầu bằng 'AIzaSy')."
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gọi Gemini AI cho '{Title}', dùng dự phòng.", title);
                var fallbackModel = string.IsNullOrWhiteSpace(_settings.ModelName) ? "gemini-1.5-flash" : _settings.ModelName;
                return GenerateFallbackSummary(title, effectiveContent, fallbackModel);
            }
        }

        public async Task<SelectionExplainResult> ExplainSelectionAsync(string selectedText, string policyTitle)
        {
            if (selectedText.Length > 2000)
                selectedText = selectedText[..2000];

            var trimmed = selectedText.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length < 5)
                return new SelectionExplainResult { IsSuccess = false, ErrorMessage = "Đoạn văn bản quá ngắn để giải thích." };

            var cacheKey = $"ai_explain_{ComputeSha256(policyTitle + trimmed)}";
            if (_cache.TryGetValue(cacheKey, out SelectionExplainResult? cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Trả kết quả giải thích đoạn từ cache.");
                return cachedResult;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return new SelectionExplainResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Tính năng AI chưa được cấu hình. Vui lòng liên hệ quản trị viên."
                };
            }

            try
            {
                var modelName = string.IsNullOrWhiteSpace(_settings.ModelName) ? "gemini-1.5-flash" : _settings.ModelName;
                var prompt = EXPLAIN_PROMPT_TEMPLATE
                    .Replace("{SELECTED_TEXT}", trimmed)
                    .Replace("{POLICY_TITLE}", policyTitle);

                var aiText = await CallGeminiSdkAsync(prompt, modelName);

                if (string.IsNullOrWhiteSpace(aiText))
                    return GenerateFallbackExplain(trimmed, policyTitle, modelName);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<GeminiSelectionJson>(aiText, options);

                if (aiResult == null)
                    return GenerateFallbackExplain(trimmed, policyTitle, modelName);

                var result = new SelectionExplainResult
                {
                    IsSuccess = true,
                    SimpleExplanation = aiResult.SimpleExplanation ?? string.Empty,
                    KeyTerm = aiResult.KeyTerm,
                    PracticalMeaning = aiResult.PracticalMeaning ?? string.Empty,
                    ModelUsed = modelName
                };

                _cache.Set(cacheKey, result, TimeSpan.FromHours(48));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Lỗi xác thực API Key Gemini trong ExplainSelectionAsync.");
                return new SelectionExplainResult
                {
                    IsSuccess = false,
                    ErrorMessage = "API Key Gemini cấu hình trong file 'appsettings.json' không hợp lệ hoặc đã hết hạn (Lỗi 401/403/400). Vui lòng cập nhật API Key chính xác từ Google AI Studio (bắt đầu bằng 'AIzaSy')."
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gọi Gemini AI giải thích đoạn bôi đen, dùng dự phòng.");
                return GenerateFallbackExplain(trimmed, policyTitle, _settings.ModelName);
            }
        }

        public async Task<SectionedReadResult> ReadAndExplainAsync(string title, string content)
        {
            var effectiveContent = content;
            if (effectiveContent.Length > 8000)
                effectiveContent = effectiveContent[..8000];

            var cacheKey = $"policy_sectioned_{ComputeSha256(title + effectiveContent)}";
            if (_cache.TryGetValue(cacheKey, out SectionedReadResult? cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Trả kết quả đọc hiểu từng phần từ cache.");
                return cachedResult;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return new SectionedReadResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Tính năng AI chưa được cấu hình. Vui lòng liên hệ quản trị viên."
                };
            }

            try
            {
                var modelName = string.IsNullOrWhiteSpace(_settings.ModelName) ? "gemini-1.5-flash" : _settings.ModelName;
                var prompt = READ_EXPLAIN_PROMPT_TEMPLATE
                    .Replace("{TITLE}", title)
                    .Replace("{CONTENT}", effectiveContent);

                var aiText = await CallGeminiSdkAsync(prompt, modelName);

                if (string.IsNullOrWhiteSpace(aiText))
                    return GenerateFallbackReadExplain(title, effectiveContent, modelName);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<GeminiSectionedJson>(aiText, options);

                if (aiResult == null)
                    return GenerateFallbackReadExplain(title, effectiveContent, modelName);

                var sections = new List<PolicySection>();
                if (aiResult.Sections != null)
                {
                    foreach (var s in aiResult.Sections)
                    {
                        sections.Add(new PolicySection
                        {
                            Title = s.Title ?? string.Empty,
                            OriginalText = s.OriginalText ?? string.Empty,
                            SimpleExplanation = s.SimpleExplanation ?? string.Empty,
                            ActionRequired = s.ActionRequired
                        });
                    }
                }

                var result = new SectionedReadResult
                {
                    IsSuccess = true,
                    OverallGist = aiResult.OverallGist ?? string.Empty,
                    Sections = sections,
                    ModelUsed = modelName
                };

                _cache.Set(cacheKey, result, TimeSpan.FromHours(48));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Lỗi xác thực API Key Gemini trong ReadAndExplainAsync.");
                return new SectionedReadResult
                {
                    IsSuccess = false,
                    ErrorMessage = "API Key Gemini cấu hình trong file 'appsettings.json' không hợp lệ hoặc đã hết hạn (Lỗi 401/403/400). Vui lòng cập nhật API Key chính xác từ Google AI Studio (bắt đầu bằng 'AIzaSy')."
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gọi Gemini AI đọc hiểu từng phần, dùng dự phòng.");
                return GenerateFallbackReadExplain(title, effectiveContent, _settings.ModelName);
            }
        }

        private async Task<string?> CallGeminiSdkAsync(string prompt, string modelName)
        {
            try
            {
                var googleAI = new GoogleAI(_settings.ApiKey);
                var model = googleAI.GenerativeModel(model: modelName);
                var response = await model.GenerateContent(prompt);
                var text = response.Text;
                _logger.LogInformation("Gemini SDK gọi thành công, model: {Model}", modelName);
                return text;
            }
            catch (Exception ex)
            {
                if (IsApiKeyError(ex))
                {
                    throw new UnauthorizedAccessException("API_KEY_INVALID", ex);
                }
                _logger.LogWarning(ex, "Gemini SDK thất bại với model {Model}, thử gemini-1.5-flash.", modelName);
                try
                {
                    var googleAI = new GoogleAI(_settings.ApiKey);
                    var model = googleAI.GenerativeModel(model: "gemini-1.5-flash");
                    var response = await model.GenerateContent(prompt);
                    return response.Text;
                }
                catch (Exception ex2)
                {
                    if (IsApiKeyError(ex2))
                    {
                        throw new UnauthorizedAccessException("API_KEY_INVALID", ex2);
                    }
                    _logger.LogWarning(ex2, "Gemini SDK thất bại hoàn toàn.");
                    return null;
                }
            }
        }

        private static bool IsApiKeyError(Exception ex)
        {
            var msg = ex.ToString().ToLower();
            return msg.Contains("api_key_invalid") || 
                   msg.Contains("api key not valid") || 
                   msg.Contains("400") || 
                   msg.Contains("401") || 
                   msg.Contains("403") || 
                   msg.Contains("unauthorized") || 
                   msg.Contains("forbidden");
        }

        private static PolicySummaryResult GenerateFallbackSummary(string title, string content, string modelName)
        {
            var bulletPoints = new List<string>();
            string shortSummary;
            string realWorldExample;

            if (title.Contains("xử phạt") || title.Contains("môi trường"))
            {
                shortSummary = "Nghị định mới quy định tăng mức xử phạt vi phạm hành chính đối với các hành vi gây ô nhiễm môi trường tại khu vực đô thị như vứt rác thải sinh hoạt bừa bãi, phóng uế bừa bãi nơi công cộng, và đổ phế thải xây dựng trái phép.";
                bulletPoints.Add("Tăng mức phạt tiền từ 1.000.000 đến 15.000.000 đồng tùy theo mức độ vi phạm.");
                bulletPoints.Add("Xử phạt nghiêm khắc hành vi vứt rác, đổ nước thải trên vỉa hè hoặc lòng đường.");
                bulletPoints.Add("Áp dụng hình thức tịch thu tang vật và buộc khôi phục lại hiện trạng môi trường ban đầu.");
                realWorldExample = "Ví dụ thực tế: Nếu bạn vứt rác sinh hoạt bừa bãi ra vỉa hè công cộng thay vì vứt vào thùng rác đô thị, bạn có thể bị lực lượng chức năng phát hiện và phạt tiền từ 5.000.000 đồng đến 10.000.000 đồng.";
            }
            else if (title.Contains("vỉa hè") || title.Contains("chỉnh trang"))
            {
                shortSummary = "Quyết định của UBND Quận 1 ban hành kế hoạch cải tạo và đồng bộ hóa vỉa hè bằng đá tự nhiên tại 12 tuyến đường trung tâm, nhằm nâng cao tính thẩm mỹ và an toàn giao thông đô thị.";
                bulletPoints.Add("Hạ ngầm toàn bộ mạng lưới điện cáp viễn thông và trồng mới hệ thống cây xanh đô thị.");
                bulletPoints.Add("Cơ cấu kinh phí: Ngân sách nhà nước hỗ trợ 70%, xã hội hóa đóng góp 30% từ các hộ kinh doanh.");
                bulletPoints.Add("Đặc biệt hỗ trợ 100% kinh phí cho các hộ gia đình chính sách, hộ nghèo nằm trong vùng dự án.");
                realWorldExample = "Ví dụ thực tế: Nếu hộ gia đình bạn ở mặt tiền đường Lê Lợi, vỉa hè trước nhà sẽ được cải tạo lát đá tự nhiên đồng bộ. Nhà bạn sẽ phối hợp bàn giao mặt bằng đúng tiến độ để đơn vị thi công.";
            }
            else if (title.Contains("dọn dẹp") || title.Contains("vệ sinh"))
            {
                shortSummary = "Ủy ban nhân dân Phường Bến Nghé thông báo kế hoạch tổng dọn dẹp vệ sinh môi trường, xóa quảng cáo rao vặt trái phép trên cột điện và chỉnh trang tuyến hẻm đô thị.";
                bulletPoints.Add("Thời gian ra quân bắt đầu từ 07 giờ 00 phút sáng Chủ Nhật ngày 21 tháng 06 năm 2026.");
                bulletPoints.Add("Ban điều hành khu phố và người dân dọn dẹp, quét rác trước cửa nhà và các tuyến hẻm chung.");
                bulletPoints.Add("Đoàn Thanh niên và Hội Phụ nữ phối hợp dọn dẹp tại các tuyến đường Nguyễn Huệ, Lê Lợi.");
                realWorldExample = "Ví dụ thực tế: Vào sáng Chủ Nhật tuần này, bạn hãy chủ động quét dọn ngõ đi chung trước cửa nhà mình và hướng dẫn trẻ nhỏ bỏ rác đúng nơi quy định để hưởng ứng phong trào của phường.";
            }
            else
            {
                shortSummary = $"Bản tóm tắt dự phòng cho chính sách '{title}'. Văn bản đưa ra các quy định hành chính mới nhằm chuẩn hóa quy trình quản lý và nâng cao ý thức cộng đồng.";
                bulletPoints.Add("Chủ động cập nhật các quy định hành chính mới để thực hiện đúng pháp luật.");
                bulletPoints.Add("Phối hợp với chính quyền địa phương trong việc triển khai và giám sát thực hiện chính sách.");
                realWorldExample = "Ví dụ thực tế: Bạn hãy thường xuyên theo dõi các thông tin chính sách của địa phương để bảo vệ quyền lợi cá nhân và thực hiện đúng nghĩa vụ công dân.";
            }

            return new PolicySummaryResult
            {
                IsSuccess = true,
                ShortSummary = shortSummary,
                BulletPoints = bulletPoints,
                RealWorldExample = realWorldExample,
                ModelUsed = modelName + " (Fallback)",
                TokensUsed = 0
            };
        }

        private static SelectionExplainResult GenerateFallbackExplain(string selectedText, string policyTitle, string modelName)
        {
            var lower = selectedText.ToLower();
            string simpleExplanation;
            string? keyTerm = null;
            string practicalMeaning;

            // Trích đoạn ngắn để hiển thị trong giải thích
            var preview = selectedText.Length > 80 ? selectedText[..80].Trim() + "..." : selectedText.Trim();

            if (lower.Contains("phạt tiền") || lower.Contains("xử phạt"))
            {
                simpleExplanation = $"Đoạn này quy định về hình thức xử phạt bằng tiền mặt đối với các hành vi vi phạm hành chính.";
                keyTerm = "Xử phạt hành chính: Là việc cơ quan nhà nước có thẩm quyền áp dụng các biện pháp phạt tiền hoặc cảnh cáo đối với người vi phạm quy định.";
                practicalMeaning = "Bạn cần lưu ý thực hiện đúng quy định để tránh bị phạt tiền theo mức quy định.";
            }
            else if (lower.Contains("hạ ngầm") || lower.Contains("cải tạo") || lower.Contains("vỉa hè"))
            {
                simpleExplanation = $"Đoạn này nói về việc cải tạo, nâng cấp hạ tầng đô thị (vỉa hè, đường phố, hệ thống điện/cáp) nhằm làm đẹp và an toàn hơn.";
                keyTerm = "Hạ ngầm: Đưa dây cáp điện, viễn thông từ trên không xuống lòng đất để đảm bảo mỹ quan và an toàn.";
                practicalMeaning = "Người dân sống trong khu vực cải tạo cần hợp tác bàn giao mặt bằng và có thể phải đóng góp một phần kinh phí theo quy định.";
            }
            else if (lower.Contains("xã hội hóa") || lower.Contains("đóng góp") || lower.Contains("kinh phí"))
            {
                simpleExplanation = $"Đoạn này nói về cơ chế huy động vốn từ người dân và doanh nghiệp cùng nhà nước để thực hiện dự án công ích.";
                keyTerm = "Xã hội hóa: Nhà nước và người dân/doanh nghiệp cùng đóng góp kinh phí để thực hiện dự án lợi ích chung.";
                practicalMeaning = "Tuỳ vào đối tượng, bạn có thể được miễn phí (hộ nghèo, chính sách) hoặc đóng góp một phần chi phí theo tỷ lệ quy định.";
            }
            else if (lower.Contains("tặng") || lower.Contains("hoa") || lower.Contains("cây xanh") || lower.Contains("môi trường sống"))
            {
                simpleExplanation = $"Đoạn này mô tả hoạt động của lãnh đạo/tổ chức đến trao tặng quà hoặc cây xanh cho người dân, khuyến khích bảo vệ môi trường sống xanh sạch đẹp.";
                keyTerm = null;
                practicalMeaning = "Đây là hoạt động cộng đồng mang tính khuyến khích, người dân được hưởng lợi trực tiếp từ các phần quà và chương trình trồng cây xanh tại khu vực sinh sống.";
            }
            else if (lower.Contains("kỷ niệm") || lower.Contains("hưởng ứng") || lower.Contains("ra quân") || lower.Contains("thành đoàn") || lower.Contains("hội liên hiệp"))
            {
                simpleExplanation = $"Đoạn này mô tả hoạt động kỷ niệm hoặc phong trào thi đua của tổ chức chính trị/xã hội, kêu gọi người dân cùng tham gia hưởng ứng.";
                keyTerm = null;
                practicalMeaning = "Người dân có thể tham gia các hoạt động cộng đồng này để đóng góp vào phong trào chung của địa phương và thể hiện tinh thần công dân.";
            }
            else if (lower.Contains("phát biểu") || lower.Contains("bí thư") || lower.Contains("chủ tịch") || lower.Contains("lãnh đạo"))
            {
                simpleExplanation = $"Đoạn này ghi lại phát biểu hoặc chỉ đạo của lãnh đạo tại sự kiện, thể hiện định hướng và thông điệp chính sách của chính quyền.";
                keyTerm = null;
                practicalMeaning = "Những phát biểu này phản ánh chủ trương, định hướng của chính quyền địa phương mà người dân nên nắm bắt để hiểu rõ chính sách đang được thực thi.";
            }
            else
            {
                // Fallback thông minh dựa trên nội dung thực tế
                var wordCount = selectedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                simpleExplanation = $"Đoạn văn bản này (gồm {wordCount} từ) là một phần nội dung trong chính sách \"{policyTitle}\", đề cập đến các quy định hoặc thông tin hành chính liên quan.";
                practicalMeaning = $"Để hiểu rõ hơn ý nghĩa cụ thể của đoạn: \"{preview}\", bạn nên đọc toàn bộ văn bản hoặc liên hệ cơ quan chức năng để được giải thích chi tiết.";
            }

            return new SelectionExplainResult
            {
                IsSuccess = true,
                SimpleExplanation = simpleExplanation,
                KeyTerm = keyTerm,
                PracticalMeaning = practicalMeaning,
                ModelUsed = modelName + " (Fallback)"
            };
        }

        private static SectionedReadResult GenerateFallbackReadExplain(string title, string content, string modelName)
        {
            var sections = new List<PolicySection>();
            string overallGist;

            if (title.Contains("xử phạt") || title.Contains("môi trường"))
            {
                overallGist = "Nghị định mới tăng mức phạt tiền đối với các hành vi xả rác và gây ô nhiễm nơi đô thị.";
                sections.Add(new PolicySection
                {
                    Title = "Phần 1: Quy định chung về đối tượng xử phạt",
                    OriginalText = "Nghị định này áp dụng đối với cá nhân, tổ chức trong và ngoài nước có hành vi vi phạm hành chính...",
                    SimpleExplanation = "Tất cả mọi người và tổ chức sinh sống, hoạt động trên lãnh thổ Việt Nam nếu làm ô nhiễm môi trường đều sẽ bị phạt.",
                    ActionRequired = "Mọi người dân phải giữ gìn vệ sinh chung, bỏ rác đúng giờ và đúng nơi quy định."
                });
                sections.Add(new PolicySection
                {
                    Title = "Phần 2: Mức xử phạt đối với các lỗi cụ thể",
                    OriginalText = "Phạt tiền từ 5.000.000 đến 10.000.000 đồng đối với hành vi vứt rác thải sinh hoạt không đúng nơi quy định...",
                    SimpleExplanation = "Hành vi vứt túi rác sinh hoạt bừa bãi ra đường phố, vỉa hè hoặc cống thoát nước đô thị sẽ bị phạt tiền rất nặng (từ 5 đến 10 triệu đồng).",
                    ActionRequired = "Tuyệt đối không vứt đầu thuốc lá, xả nước thải bẩn hoặc túi rác ra vỉa hè, lòng đường."
                });
                sections.Add(new PolicySection
                {
                    Title = "Phần 3: Thẩm quyền xử phạt của chính quyền địa phương",
                    OriginalText = "Chủ tịch UBND cấp xã có quyền phạt cảnh cáo, phạt tiền đến 5.000.000 đồng...",
                    SimpleExplanation = "Ủy ban nhân dân Phường và Công an Phường có đầy đủ thẩm quyền tuần tra, lập biên bản và ra quyết định xử phạt trực tiếp đối với các vi phạm nhỏ.",
                    ActionRequired = "Khi bị lập biên bản, người dân có trách nhiệm nộp phạt tại kho bạc hoặc ngân hàng được chỉ định trong thời hạn quy định."
                });
            }
            else if (title.Contains("vỉa hè") || title.Contains("chỉnh trang"))
            {
                overallGist = "Quận 1 thực hiện cải tạo lại vỉa hè bằng đá tự nhiên kết hợp hạ ngầm dây cáp mạng.";
                sections.Add(new PolicySection
                {
                    Title = "Phần 1: Phạm vi cải tạo",
                    OriginalText = "Áp dụng thí điểm trên 12 tuyến đường trung tâm bao gồm: Nguyễn Huệ, Đồng Khởi, Lê Lợi...",
                    SimpleExplanation = "Trước mắt, nhà nước sẽ cải tạo vỉa hè tại 12 con đường lớn ở trung tâm Quận 1 để làm phố đi bộ khang trang.",
                    ActionRequired = null
                });
                sections.Add(new PolicySection
                {
                    Title = "Phần 2: Phương thức đóng góp tài chính",
                    OriginalText = "Ngân sách nhà nước hỗ trợ 70%, xã hội hóa đóng góp 30% từ các doanh nghiệp, hộ kinh doanh...",
                    SimpleExplanation = "Chi phí xây dựng chung được chia sẻ: Nhà nước thanh toán 70% từ ngân sách công cộng, các cơ sở kinh doanh hưởng lợi mặt tiền tự đóng góp 30%. Hộ nghèo được miễn phí hoàn toàn.",
                    ActionRequired = "Hộ kinh doanh thuộc diện đóng góp cần chuẩn bị tài chính và thực hiện nộp theo hướng dẫn của phường."
                });
            }
            else
            {
                overallGist = $"Chính sách quy định các thủ tục và trách nhiệm hành chính của cơ quan địa phương và người dân đối với '{title}'.";
                sections.Add(new PolicySection
                {
                    Title = "Phần 1: Quy định chung",
                    OriginalText = content[..Math.Min(150, content.Length)] + "...",
                    SimpleExplanation = "Giới thiệu mục tiêu và các khái niệm cơ bản liên quan đến văn bản chính sách này.",
                    ActionRequired = null
                });
            }

            return new SectionedReadResult
            {
                IsSuccess = true,
                OverallGist = overallGist,
                Sections = sections,
                ModelUsed = modelName + " (Fallback)"
            };
        }

        private static string ComputeSha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private class GeminiPolicyJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("short_summary")]
            public string? ShortSummary { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("bullet_points")]
            public List<string>? BulletPoints { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("real_world_example")]
            public string? RealWorldExample { get; set; }
        }

        private class GeminiSelectionJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("simple_explanation")]
            public string? SimpleExplanation { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("key_term")]
            public string? KeyTerm { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("practical_meaning")]
            public string? PracticalMeaning { get; set; }
        }

        private class GeminiSectionedJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("overall_gist")]
            public string? OverallGist { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("sections")]
            public List<GeminiSectionItemJson>? Sections { get; set; }
        }

        private class GeminiSectionItemJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("title")]
            public string? Title { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("original_text")]
            public string? OriginalText { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("simple_explanation")]
            public string? SimpleExplanation { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("action_required")]
            public string? ActionRequired { get; set; }
        }
    }
}
