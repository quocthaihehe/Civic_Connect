using CivicConnect.Core.Entities;
using CivicConnect.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface IIssueService
    {
        Task<Issue?> GetIssueByIdAsync(int id);
        Task<IEnumerable<Issue>> GetIssuesForUserAsync(string userId);
        Task<IEnumerable<Issue>> GetIssuesForOfficialAsync(string userId);
        Task<IEnumerable<Issue>> GetMapDataAsync(IssueCategory? category, IssueStatus? status, double? lat, double? lng, double radiusKm);
        Task<Issue> CreateIssueAsync(Issue issue, List<IssueImage> images);
        Task<bool> VoteIssueAsync(int issueId, string userId, VoteType type);
        Task<Comment> AddCommentAsync(int issueId, string authorId, string content, int? parentCommentId, bool isOfficial);
        
        // Official Workflows
        Task<bool> AcceptIssueAsync(int issueId, string officialId);
        Task<bool> ProcessIssueAsync(int issueId, string officialId);
        Task<bool> ResolveIssueAsync(int issueId, string officialId, string? note, string? attachmentUrl);
        Task<bool> RejectIssueAsync(int issueId, string officialId, string reason);
        Task<bool> EscalateIssueAsync(int issueId, string officialId, string note);
        Task<bool> AssignIssueAsync(int issueId, string officialId, string assignedToUserId, string? note);

        // Background Jobs
        Task UpdatePriorityScoresAsync();
        Task CheckDeadlinesAsync();
    }
}
