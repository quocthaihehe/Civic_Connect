using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using CivicConnect.Web.Hubs;

namespace CivicConnect.Web.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public IssueService(
            IIssueRepository issueRepository,
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _issueRepository = issueRepository;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public async Task<Issue?> GetIssueByIdAsync(int id)
        {
            return await _issueRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Issue>> GetIssuesForUserAsync(string userId)
        {
            return await _context.Issues
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .Where(i => i.AuthorId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetIssuesForOfficialAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Enumerable.Empty<Issue>();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                return await _issueRepository.GetAllAsync();
            }
            if (roles.Contains("OfficialDistrict"))
            {
                return await _issueRepository.GetByDistrictAsync(user.DistrictCode ?? "");
            }
            if (roles.Contains("OfficialWard"))
            {
                return await _issueRepository.GetByWardAsync(user.WardCode ?? "");
            }

            return await _issueRepository.GetPublicAsync();
        }

        public async Task<IEnumerable<Issue>> GetMapDataAsync(
            IssueCategory? category, IssueStatus? status, double? lat, double? lng, double radiusKm)
        {
            var query = _context.Issues
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(i => i.Category == category.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var issues = await query.ToListAsync();

            if (lat.HasValue && lng.HasValue)
            {
                return issues.Where(i => CalculateDistance(lat.Value, lng.Value, i.Latitude, i.Longitude) <= radiusKm);
            }

            return issues;
        }

        public async Task<Issue> CreateIssueAsync(Issue issue, List<IssueImage> images)
        {
            // Validation 1 (A2 - Geo-fencing)
            if (!string.IsNullOrEmpty(issue.ProvinceCode))
            {
                var boundary = await _context.ProvinceBoundaries
                    .FirstOrDefaultAsync(b => b.ProvinceCode == issue.ProvinceCode);
                
                if (boundary != null)
                {
                    if (issue.Latitude < boundary.MinLat || issue.Latitude > boundary.MaxLat ||
                        issue.Longitude < boundary.MinLng || issue.Longitude > boundary.MaxLng)
                    {
                        throw new ArgumentException("Tọa độ không hợp lệ hoặc nằm ngoài ranh giới cho phép");
                    }
                }
            }

            // Validation 2 (A7 - Duplicate Check)
            var thresholdDate = DateTime.UtcNow.AddDays(-30);
            var similarIssues = await _context.Issues
                .Where(i => i.Category == issue.Category &&
                            i.WardCode == issue.WardCode &&
                            i.CreatedAt >= thresholdDate &&
                            (i.Status == IssueStatus.Pending || i.Status == IssueStatus.Assigned || i.Status == IssueStatus.Processing))
                .ToListAsync();

            bool isDuplicate = false;
            foreach (var existingIssue in similarIssues)
            {
                var distance = CalculateDistance(issue.Latitude, issue.Longitude, existingIssue.Latitude, existingIssue.Longitude);
                if (distance <= 0.05)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate)
            {
                issue.Description = $"[CẢNH BÁO: Trùng lặp phản ánh trong bán kính 50m]\n{issue.Description}";
            }

            issue.Status = IssueStatus.Pending;
            issue.DueDate = CalculateDueDate(issue.Priority);
            issue.CreatedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;

            await _issueRepository.AddAsync(issue);
            await _issueRepository.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    images[i].IssueId = issue.Id;
                    images[i].OrderIndex = i;
                    await _context.IssueImages.AddAsync(images[i]);
                }
                await _issueRepository.SaveChangesAsync();
            }

            // Ghi nhận lịch sử trạng thái đầu tiên
            var history = new IssueStatusHistory
            {
                IssueId = issue.Id,
                FromStatus = IssueStatus.Pending,
                ToStatus = IssueStatus.Pending,
                ChangedById = issue.AuthorId,
                Note = "Khởi tạo phản ánh mới.",
                ChangedAt = DateTime.UtcNow
            };
            await _context.IssueStatusHistories.AddAsync(history);
            await _context.SaveChangesAsync();

            await BroadcastStatsAsync();

            // Gửi thông báo cho người dùng
            if (!string.IsNullOrEmpty(issue.AuthorId))
            {
                await _notificationService.SendNotificationAsync(
                    issue.AuthorId,
                    "Gửi phản ánh thành công",
                    $"Phản ánh '{issue.Title}' của bạn đã được hệ thống ghi nhận và đang chờ phân công xử lý.",
                    NotificationType.IssueStatusChanged,
                    issue.Id.ToString()
                );
            }

            // Gửi thông báo cho toàn bộ Admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                await _notificationService.SendNotificationAsync(
                    admin.Id,
                    "Có phản ánh mới",
                    $"Một phản ánh mới '{issue.Title}' vừa được gửi lên hệ thống.",
                    NotificationType.IssueStatusChanged,
                    issue.Id.ToString()
                );
            }

            return issue;
        }

        public async Task<bool> VoteIssueAsync(int issueId, string userId, VoteType type)
        {
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.IssueId == issueId);

            if (existingVote != null)
            {
                if (existingVote.Type == type)
                {
                    // Há»§y vote náº¿u click láº¡i cÃ¹ng loáº¡i
                    _context.Votes.Remove(existingVote);
                }
                else
                {
                    // Thay Ä‘á»•i loáº¡i vote
                    existingVote.Type = type;
                    existingVote.CreatedAt = DateTime.UtcNow;
                    _context.Entry(existingVote).State = EntityState.Modified;
                }
            }
            else
            {
                // ThÃªm vote má»›i
                var vote = new Vote
                {
                    UserId = userId,
                    IssueId = issueId,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Votes.AddAsync(vote);
            }

            await _context.SaveChangesAsync();
            await RecalculatePriorityScoreAsync(issueId);
            return true;
        }

        public async Task<Comment> AddCommentAsync(
            int issueId, string authorId, string content, int? parentCommentId, bool isOfficial)
        {
            var comment = new Comment
            {
                IssueId = issueId,
                AuthorId = authorId,
                Content = content,
                ParentCommentId = parentCommentId,
                IsOfficialResponse = isOfficial,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Cáº­p nháº­t thá» i gian thay Ä‘á»•i pháº£n Ã¡nh
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue != null)
            {
                issue.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Gá»­i thÃ´ng bÃ¡o cho ngÆ°á» i viáº¿t pháº£n Ã¡nh náº¿u cÃ³ comment má»›i tá»« cÃ¡n bá»™
                if (isOfficial && issue.AuthorId != authorId)
                {
                    await _notificationService.SendNotificationAsync(
                        issue.AuthorId,
                        "CÆ¡ quan pháº£n há»“i pháº£n Ã¡nh cá»§a báº¡n",
                        $"CÃ¡n bá»™ xá»­ lÃ½ Ä‘Ã£ gá»­i má»™t pháº£n há»“i chÃ­nh thá»©c cho pháº£n Ã¡nh: '{issue.Title}'",
                        NotificationType.CommentAdded,
                        issue.Id.ToString()
                    );
                }
            }

            return comment;
        }

        public async Task<bool> AcceptIssueAsync(int issueId, string officialId)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null || issue.Status != IssueStatus.Pending) return false;

            var oldStatus = issue.Status;
            issue.Status = IssueStatus.Assigned;
            issue.AssignedToUserId = officialId;
            issue.AssignedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Assigned, officialId, "Tiếp nhận phản ánh.");

            // F3: Thông báo bắt buộc mốc 1 — Tiếp nhận
            await _notificationService.SendNotificationAsync(
                issue.AuthorId,
                "Phản ánh đã được tiếp nhận",
                $"Phản ánh '{issue.Title}' của bạn đã được cán bộ tiếp nhận và đang được phân công xử lý.",
                NotificationType.IssueStatusChanged,
                issue.Id.ToString()
            );

            return true;
        }

        public async Task<bool> ProcessIssueAsync(int issueId, string officialId)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null || (issue.Status != IssueStatus.Assigned && issue.Status != IssueStatus.Pending)) return false;

            // B5: Phải có assignee trước khi Processing
            if (string.IsNullOrEmpty(issue.AssignedToUserId)) return false;

            var oldStatus = issue.Status;
            issue.Status = IssueStatus.Processing;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Processing, officialId, "Bắt đầu xử lý phản ánh.");

            // F3: Thông báo bắt buộc mốc 2 — Cập nhật tiến độ
            await _notificationService.SendNotificationAsync(
                issue.AuthorId,
                "Phản ánh đang được xử lý",
                $"Phản ánh '{issue.Title}' của bạn hiện đang được tiến hành xử lý bởi cán bộ phụ trách.",
                NotificationType.IssueStatusChanged,
                issue.Id.ToString()
            );

            return true;
        }

        public async Task<bool> ResolveIssueAsync(int issueId, string officialId, string? note, string? attachmentUrl)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null || issue.Status != IssueStatus.Processing) return false;

            // B3: Bắt buộc phải có minh chứng trước khi Resolve
            bool hasProof = !string.IsNullOrEmpty(issue.ResolutionImageUrl)
                || !string.IsNullOrEmpty(issue.ResolutionDocumentUrl)
                || !string.IsNullOrEmpty(attachmentUrl);
            if (!hasProof) return false;

            var oldStatus = issue.Status;
            issue.Status = IssueStatus.Resolved;
            issue.ResolvedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;

            // Cộng điểm uy tín cho người tạo phản ánh (+10 điểm)
            var author = await _userManager.FindByIdAsync(issue.AuthorId);
            if (author != null)
            {
                author.TrustScore += 10;
                _context.Entry(author).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Resolved, officialId, note ?? "Phản ánh đã được giải quyết thành công.", attachmentUrl);

            // F3: Thông báo bắt buộc mốc 3 — Đóng phản ánh
            await _notificationService.SendNotificationAsync(
                issue.AuthorId,
                "Phản ánh đã được giải quyết",
                $"Phản ánh '{issue.Title}' của bạn đã được đánh dấu là Đã giải quyết. Cảm ơn bạn đã phản ánh!",
                NotificationType.IssueStatusChanged,
                issue.Id.ToString()
            );

            await BroadcastStatsAsync();

            return true;
        }

        public async Task<bool> RejectIssueAsync(int issueId, string officialId, string reason)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null || issue.Status == IssueStatus.Resolved || issue.Status == IssueStatus.Closed) return false;

            var oldStatus = issue.Status;
            issue.Status = IssueStatus.Rejected;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Rejected, officialId, $"Tá»« chá»‘i pháº£n Ã¡nh. LÃ½ do: {reason}");

            await _notificationService.SendNotificationAsync(
                issue.AuthorId,
                "Phản ánh bị từ chối",
                $"Phản ánh '{issue.Title}' của bạn đã bị từ chối. Lý do: {reason}",
                NotificationType.IssueStatusChanged,
                issue.Id.ToString()
            );

            return true;
        }

        public async Task<bool> EscalateIssueAsync(int issueId, string officialId, string note)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null) return false;

            // Chá»‰ cho phÃ©p chuyá»ƒn cáº¥p tá»« PhÆ°á» ng lÃªn Quáº­n
            var official = await _userManager.FindByIdAsync(officialId);
            if (official == null || string.IsNullOrEmpty(official.DistrictCode)) return false;

            var oldStatus = issue.Status;
            
            // 1. Chuyá»ƒn tráº¡ng thÃ¡i gá»‘c sang Assigned (hoáº·c Ä‘Ã³ng láº¡i ghi nháº­n bÃ n giao)
            issue.Status = IssueStatus.Assigned;
            issue.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 1. Chuyển trạng thái gốc sang Assigned (hoặc đóng lại ghi nhận bàn giao)
            issue.Status = IssueStatus.Assigned;
            issue.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Assigned, officialId, $"Chuyển lên cấp Quận. Lý do: {note}");

            // 2. Tạo một Issue mới ở cấp Quận (DistrictLevel)
            var escalatedIssue = new Issue
            {
                Title = $"[CHUYỂN CẤP] {issue.Title}",
                Description = $"[Phản ánh gốc #{issue.Id}] - Người chuyển cấp: {official.FullName}\nGhi chú: {note}\n\nNội dung gốc:\n{issue.Description}",
                Category = issue.Category,
                Status = IssueStatus.Pending,
                Priority = issue.Priority,
                SeverityScore = issue.SeverityScore,
                Latitude = issue.Latitude,
                Longitude = issue.Longitude,
                Address = issue.Address,
                WardCode = issue.WardCode,
                WardName = issue.WardName,
                DistrictCode = issue.DistrictCode,
                DistrictName = issue.DistrictName,
                ProvinceCode = issue.ProvinceCode,
                ProvinceName = issue.ProvinceName,
                AuthorId = issue.AuthorId,
                IsAnonymous = issue.IsAnonymous,
                IsVerified = true,
                ParentIssueId = issue.Id, // LiÃªn káº¿t vá»  issue gá»‘c
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            escalatedIssue.DueDate = CalculateDueDate(escalatedIssue.Priority);

            await _context.Issues.AddAsync(escalatedIssue);
            await _context.SaveChangesAsync();

            // Sao chÃ©p hÃ¬nh áº£nh sang issue má»›i
            foreach (var img in issue.Images)
            {
                var newImg = new IssueImage
                {
                    IssueId = escalatedIssue.Id,
                    PublicId = img.PublicId,
                    Url = img.Url,
                    ThumbnailUrl = img.ThumbnailUrl,
                    OriginalFileName = img.OriginalFileName,
                    FileSizeBytes = img.FileSizeBytes,
                    OrderIndex = img.OrderIndex,
                    UploadedAt = DateTime.UtcNow
                };
                await _context.IssueImages.AddAsync(newImg);
            }
            await _context.SaveChangesAsync();

            // Ghi nháº­n lá»‹ch sá»­ issue má»›i
            await SaveHistoryAsync(escalatedIssue.Id, IssueStatus.Pending, IssueStatus.Pending, officialId, $"Khá»Ÿi táº¡o tá»« luá»“ng chuyá»ƒn cáº¥p pháº£n Ã¡nh #{issue.Id}.");

            // Gá»­i thÃ´ng bÃ¡o cho cÃ´ng dÃ¢n
            await _notificationService.SendNotificationAsync(
                issue.AuthorId,
                "Phản ánh đã được chuyển lên cấp xử lý cao hơn",
                $"Phản ánh '{issue.Title}' của bạn đã được chuyển lên cơ quan cấp Quận để tiếp tục xử lý.",
                NotificationType.IssueStatusChanged,
                escalatedIssue.Id.ToString()
            );

            return true;
        }

        public async Task<bool> AssignIssueAsync(int issueId, string officialId, string assignedToUserId, string? note)
        {
            var issue = await _issueRepository.GetByIdAsync(issueId);
            if (issue == null) return false;

            // B7: Kiểm tra giới hạn tải công việc (tối đa 10 phản ánh InProgress/người)
            var inProgressCount = await _context.Issues
                .CountAsync(i => i.AssignedToUserId == assignedToUserId
                    && (i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned));
            if (inProgressCount >= 10)
            {
                // Không chặn cứng, chỉ ghi nhận cảnh báo trong Note
                note = (note ?? "") + " [CẢNH BÁO: Cán bộ đã có đủ 10 phản ánh đang xử lý]";
            }

            var oldStatus = issue.Status;
            issue.Status = IssueStatus.Assigned;
            issue.AssignedToUserId = assignedToUserId;
            issue.AssignedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SaveHistoryAsync(issueId, oldStatus, IssueStatus.Assigned, officialId, note ?? "Phân công xử lý.");

            await _notificationService.SendNotificationAsync(
                assignedToUserId,
                "Báº¡n Ä‘Æ°á»£c phÃ¢n cÃ´ng má»™t pháº£n Ã¡nh",
                $"Báº¡n Ä‘Æ°á»£c phÃ¢n cÃ´ng phá»¥ trÃ¡ch giáº£i quyáº¿t pháº£n Ã¡nh: '{issue.Title}'",
                NotificationType.IssueAssigned,
                issue.Id.ToString()
            );

            return true;
        }

        public async Task UpdatePriorityScoresAsync()
        {
            var activeIssues = await _context.Issues
                .Include(i => i.Votes)
                .Where(i => i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Closed)
                .ToListAsync();

            foreach (var issue in activeIssues)
            {
                var upvotes = issue.Votes.Count(v => v.Type == VoteType.Up);
                var downvotes = issue.Votes.Count(v => v.Type == VoteType.Down);
                var voteScore = Math.Max(0, upvotes - downvotes);

                var density = await CalculateAreaDensityAsync(issue.Latitude, issue.Longitude, 2.0);

                issue.PriorityScore = (voteScore * 0.5f) + (issue.SeverityScore * 0.3f) + (density * 0.2f);
                issue.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckDeadlinesAsync()
        {
            var overdueIssues = await _context.Issues
                .Where(i => i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Closed)
                .Where(i => i.DueDate.HasValue && i.DueDate.Value < DateTime.UtcNow)
                .ToListAsync();

            foreach (var issue in overdueIssues)
            {
                var oldStatus = issue.Status;
                issue.Status = IssueStatus.Closed;
                issue.UpdatedAt = DateTime.UtcNow;

                // Lưu lịch sử quá hạn
                var history = new IssueStatusHistory
                {
                    IssueId = issue.Id,
                    FromStatus = oldStatus,
                    ToStatus = IssueStatus.Closed,
                    ChangedById = issue.AuthorId, // Đóng tự động bởi hệ thống nhân danh tác giả
                    Note = "Hệ thống tự động đóng phản ánh do quá thời hạn xử lý quy định.",
                    ChangedAt = DateTime.UtcNow
                };
                await _context.IssueStatusHistories.AddAsync(history);

                await _notificationService.SendNotificationAsync(
                    issue.AuthorId,
                    "Phản ánh tự động đóng",
                    $"Phản ánh '{issue.Title}' của bạn đã quá hạn xử lý và tự động chuyển sang trạng thái Đóng.",
                    NotificationType.IssueStatusChanged,
                    issue.Id.ToString()
                );
            }

            await _context.SaveChangesAsync();
        }

        private async Task BroadcastStatsAsync()
        {
            try {
                var totalUsers = await _context.Users.CountAsync();
                var totalIssues = await _context.Issues.CountAsync();
                var resolvedIssues = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed);
                
                var ratedIssuesCount = await _context.Issues.CountAsync(i => i.Rating.HasValue);
                var highRatedCount = await _context.Issues.CountAsync(i => i.Rating.HasValue && i.Rating.Value >= 4);
                var satisfactionRate = ratedIssuesCount > 0 ? (int)Math.Round((double)highRatedCount / ratedIssuesCount * 100) : 100;

                await _hubContext.Clients.All.SendAsync("UpdateStats", totalUsers, totalIssues, resolvedIssues, satisfactionRate);
            } catch { }
        }

        // --- Helper Methods ---

        private async Task SaveHistoryAsync(int issueId, IssueStatus from, IssueStatus to, string userId, string note, string? attachmentUrl = null)
        {
            var history = new IssueStatusHistory
            {
                IssueId = issueId,
                FromStatus = from,
                ToStatus = to,
                ChangedById = userId,
                Note = note,
                AttachmentUrl = attachmentUrl,
                ChangedAt = DateTime.UtcNow
            };
            await _context.IssueStatusHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        private async Task RecalculatePriorityScoreAsync(int issueId)
        {
            var issue = await _context.Issues.Include(i => i.Votes).FirstOrDefaultAsync(i => i.Id == issueId);
            if (issue == null) return;

            var upvotes = issue.Votes.Count(v => v.Type == VoteType.Up);
            var downvotes = issue.Votes.Count(v => v.Type == VoteType.Down);
            var voteScore = Math.Max(0, upvotes - downvotes);

            var density = await CalculateAreaDensityAsync(issue.Latitude, issue.Longitude, 2.0);

            issue.PriorityScore = (voteScore * 0.5f) + (issue.SeverityScore * 0.3f) + (density * 0.2f);
            await _context.SaveChangesAsync();
        }

        private async Task<float> CalculateAreaDensityAsync(double lat, double lng, double radiusKm)
        {
            // TÃ­nh sá»‘ lÆ°á»£ng issue khÃ¡c trong cÃ¹ng bÃ¡n kÃ­nh 2km lÃ m trá» ng sá»‘ density (tÃ­nh toÃ¡n á»Ÿ client Ä‘á»ƒ trÃ¡nh lá»—i dá»‹ch LINQ sang SQL)
            var activeIssues = await _context.Issues
                .Where(i => i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Closed)
                .Select(i => new { i.Latitude, i.Longitude })
                .ToListAsync();

            var count = activeIssues.Count(i => CalculateDistance(lat, lng, i.Latitude, i.Longitude) <= radiusKm);
            return (float)count;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var rLat1 = lat1 * Math.PI / 180.0;
            var rLat2 = lat2 * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLat1) * Math.Cos(rLat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371.0 * c; // Khoáº£ng cÃ¡ch theo km
        }

        private DateTime CalculateDueDate(IssuePriority priority)
        {
            int days = priority switch
            {
                IssuePriority.Critical => 3,   // 3 ngÃ y lÃ m viá»‡c
                IssuePriority.High => 7,       // 7 ngÃ y lÃ m viá»‡c
                IssuePriority.Medium => 15,    // 15 ngÃ y lÃ m viá»‡c
                IssuePriority.Low => 30,       // 30 ngÃ y lÃ m viá»‡c
                _ => 15
            };
            return AddBusinessDays(DateTime.UtcNow, days);
        }

        private DateTime AddBusinessDays(DateTime start, int days)
        {
            var current = start;
            for (int i = 0; i < days;)
            {
                current = current.AddDays(1);
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    i++;
                }
            }
            return current;
        }
    }
}
