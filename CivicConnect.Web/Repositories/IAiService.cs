using CivicConnect.Web.Models.Ai;
using System.Threading.Tasks;

namespace CivicConnect.Web.Repositories
{
    public interface IAiService
    {
        Task<PolicySummaryResult> SummarizePolicyAsync(string title, string content);

        /// <summary>
        /// Giải thích một đoạn văn bản được người dùng bôi đen, trong ngữ cảnh của chính sách.
        /// </summary>
        Task<SelectionExplainResult> ExplainSelectionAsync(string selectedText, string policyTitle);

        /// <summary>
        /// Đọc toàn bộ chính sách, chia thành 3–5 phần và giải thích từng phần
        /// bằng ngôn ngữ đơn giản dành cho người dân.
        /// </summary>
        Task<SectionedReadResult> ReadAndExplainAsync(string title, string content);
    }
}
