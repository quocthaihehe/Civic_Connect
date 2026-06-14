namespace CivicConnect.Web.Models.Ai
{
    /// <summary>
    /// Kết quả trả về khi AI giải thích một đoạn văn bản người dùng bôi đen.
    /// </summary>
    public class SelectionExplainResult
    {
        public bool IsSuccess { get; set; }

        /// <summary>Giải thích đơn giản đoạn văn bản được chọn.</summary>
        public string SimpleExplanation { get; set; } = string.Empty;

        /// <summary>Thuật ngữ khó trong đoạn được giải thích (nếu có).</summary>
        public string? KeyTerm { get; set; }

        /// <summary>Ý nghĩa thực tế với người dân thường.</summary>
        public string PracticalMeaning { get; set; } = string.Empty;

        public string ModelUsed { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
