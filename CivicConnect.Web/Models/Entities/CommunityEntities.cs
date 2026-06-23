using CivicConnect.Web.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class ForumPost
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Content { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Upvotes { get; set; }
        public string Tags { get; set; } // e.g. "giaothong, an_ninh"
        
        // Community Threads upgrades
        public CivicConnect.Web.Models.Enums.PostStatus Status { get; set; } = CivicConnect.Web.Models.Enums.PostStatus.Pending;
        public string? RejectionReason { get; set; }
        
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public float PopularityScore { get; set; }
        public DateTime? EnteredTrendingAt { get; set; }
        
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string PostType { get; set; } = "Text"; // "Text", "Image", "Video", "Issue"
        public int? RelatedIssueId { get; set; }
        public Issue? RelatedIssue { get; set; }

        public List<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }

    public class ForumComment
    {
        public int Id { get; set; }
        
        public int? PostId { get; set; }
        public ForumPost? Post { get; set; }
        
        public int? IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        public int? ParentCommentId { get; set; }
        public ForumComment? ParentComment { get; set; }
        
        public int Depth { get; set; } = 0;
        public int LikeCount { get; set; }
        
        [Required] public string Content { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public List<ForumComment> Replies { get; set; } = new List<ForumComment>();
    }

    public class Poll
    {
        public int Id { get; set; }
        [Required] public string Question { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public List<PollOption> Options { get; set; } = new List<PollOption>();
        public List<PollVote> Votes { get; set; } = new List<PollVote>();
    }

    public class PollOption
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        [Required] public string Text { get; set; }
        public int VoteCount { get; set; }
    }

    public class PollVote
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public int OptionId { get; set; }
        public PollOption Option { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }

    public class Petition
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        public string TargetAudience { get; set; }
        public int TargetSignatures { get; set; }
        public int CurrentSignatures { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public string Status { get; set; } 
    }

    public class PetitionSignature
    {
        public int Id { get; set; }
        public int PetitionId { get; set; }
        public Petition Petition { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
    }

    public class CommunityEvent
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Organizer { get; set; }
        public string ImageUrl { get; set; }
        public int MaxParticipants { get; set; }
        public List<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }

    public class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public CommunityEvent Event { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
