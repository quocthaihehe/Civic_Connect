using System;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class TrendingTopic
    {
        public int Id { get; set; }
        
        [Required]
        public string Tag { get; set; }
        
        public int PostCount { get; set; } // Số lượng bài viết sử dụng tag này trong 7 ngày qua
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
