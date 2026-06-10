using CivicConnect.Core.Enums;
using System;

namespace CivicConnect.Core.Entities
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
