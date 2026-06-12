using System;
using System.Collections.Generic;

namespace CivicConnect.Web.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public Issue Issue { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public string Content { get; set; }
        public int? ParentCommentId { get; set; }    // Hỗ trợ bình luận lồng nhau (Reply)
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public bool IsOfficialResponse { get; set; } // Phản hồi chính thức từ cơ quan
        public bool IsHidden { get; set; } = false;  // Ẩn bình luận (nếu vi phạm chính sách)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
    }
}
