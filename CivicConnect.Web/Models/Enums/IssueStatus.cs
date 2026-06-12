namespace CivicConnect.Web.Models.Enums
{
    public enum IssueStatus
    {
        Pending = 0,      // Chờ tiếp nhận
        Assigned = 1,     // Đã phân công
        Processing = 2,   // Đang xử lý
        Resolved = 3,     // Đã giải quyết
        Rejected = 4,     // Từ chối / không hợp lệ
        Closed = 5        // Đóng (quá hạn không phản hồi)
    }
}
