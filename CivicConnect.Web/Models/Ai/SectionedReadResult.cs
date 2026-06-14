using System.Collections.Generic;

namespace CivicConnect.Web.Models.Ai
{
    /// <summary>
    /// Đại diện một phần/section trong kết quả "AI Đọc &amp; Giải Thích"
    /// </summary>
    public class PolicySection
    {
        /// <summary>Tiêu đề phần, ví dụ: "Phần 1: Quy định chung"</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Trích dẫn/tóm lược đoạn nội dung gốc tương ứng</summary>
        public string OriginalText { get; set; } = string.Empty;

        /// <summary>Giải thích bằng ngôn ngữ đơn giản, dễ hiểu</summary>
        public string SimpleExplanation { get; set; } = string.Empty;

        /// <summary>Hành động người dân cần thực hiện (nullable — không phải lúc nào cũng có)</summary>
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Kết quả của tính năng "AI Đọc &amp; Giải Thích" — chia chính sách thành các phần,
    /// giải thích từng phần bằng ngôn ngữ đơn giản.
    /// </summary>
    public class SectionedReadResult
    {
        public bool IsSuccess { get; set; }

        /// <summary>Một câu tóm tắt cực ngắn toàn bộ chính sách</summary>
        public string OverallGist { get; set; } = string.Empty;

        /// <summary>Danh sách các phần đã được AI phân tích và giải thích</summary>
        public List<PolicySection> Sections { get; set; } = new();

        public string ModelUsed { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
