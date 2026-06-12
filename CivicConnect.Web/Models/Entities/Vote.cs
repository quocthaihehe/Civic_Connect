using CivicConnect.Web.Models.Enums;
using System;

namespace CivicConnect.Web.Models.Entities
{
    public class Vote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int IssueId { get; set; }
        public VoteType Type { get; set; } // Up / Down
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
