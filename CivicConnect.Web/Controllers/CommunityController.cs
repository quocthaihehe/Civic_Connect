using CivicConnect.Web.Data;
using CivicConnect.Web.Hubs;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CivicConnect.Web.Controllers
{
    public class CommunityController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICitizenPointsService _pointsService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommunityController(AppDbContext context, UserManager<ApplicationUser> userManager, ICitizenPointsService pointsService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _pointsService = pointsService;
            _hubContext = hubContext;
        }

        // 1. MẠNG XÃ HỘI HUB (Index) - Mixed Feed
        public async Task<IActionResult> Index(string tab = "newest")
        {
            // Trộn Posts (Approved) và Issues (Processing, Resolved)
            var posts = await _context.ForumPosts
                .Include(p => p.Author)
                .Where(p => p.Status == PostStatus.Approved)
                .ToListAsync();

            var issues = await _context.Issues
                .Include(i => i.Author)
                .Where(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Resolved)
                .ToListAsync();

            // Định dạng chung để gộp
            var mixedFeed = new List<dynamic>();
            
            foreach (var post in posts)
            {
                mixedFeed.Add(new {
                    Type = "Post",
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    AuthorName = post.Author.FullName ?? post.Author.UserName,
                    AuthorId = post.AuthorId,
                    CreatedAt = post.CreatedAt,
                    LikeCount = post.LikeCount,
                    CommentCount = post.CommentCount,
                    PopularityScore = post.PopularityScore,
                    Tags = post.Tags
                });
            }

            foreach (var issue in issues)
            {
                // Lấy comment count cho issue
                var commentCount = await _context.Comments.CountAsync(c => c.IssueId == issue.Id) + 
                                   await _context.ForumComments.CountAsync(c => c.IssueId == issue.Id);
                mixedFeed.Add(new {
                    Type = "Issue",
                    Id = issue.Id,
                    Title = issue.Title,
                    Content = issue.Description,
                    AuthorName = issue.IsAnonymous ? "Người dân ẩn danh" : (issue.Author?.FullName ?? issue.Author?.UserName),
                    AuthorId = issue.AuthorId,
                    CreatedAt = issue.CreatedAt,
                    LikeCount = 0, // Issues ko có like
                    CommentCount = commentCount,
                    PopularityScore = 0f,
                    Status = issue.Status,
                    Address = issue.Address
                });
            }

            // Sắp xếp theo tab
            if (tab == "trending")
            {
                mixedFeed = mixedFeed.OrderByDescending(x => (float)x.PopularityScore).ToList();
            }
            else
            {
                mixedFeed = mixedFeed.OrderByDescending(x => (DateTime)x.CreatedAt).ToList();
            }

            // Dữ liệu cột phải
            ViewBag.TrendingTopics = await _context.TrendingTopics
                .OrderByDescending(t => t.PostCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TopCitizens = await _context.Users
                .OrderByDescending(u => u.CitizenPoints)
                .Take(5)
                .Select(u => new { u.Id, u.FullName, u.UserName, u.CitizenPoints })
                .ToListAsync();

            ViewBag.CurrentTab = tab;
            return View(mixedFeed);
        }

        // Chi tiết bài viết (PostDetail)
        public async Task<IActionResult> PostDetail(int id)
        {
            var post = await _context.ForumPosts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == PostStatus.Approved);

            if (post == null) return NotFound();

            var comments = await _context.ForumComments
                .Include(c => c.Author)
                .Include(c => c.Replies).ThenInclude(r => r.Author)
                .Where(c => c.PostId == id && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Comments = comments;
            return View(post);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(string title, string content, string tags)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content)) return BadRequest();

            var post = new ForumPost
            {
                Title = title,
                Content = content,
                Tags = tags,
                AuthorId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = PostStatus.Pending // Chờ duyệt
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Chuyển về Index kèm thông báo "Đã gửi chờ duyệt"
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(int? postId, int? issueId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(content)) return Json(new { success = false, message = "Lỗi" });

            var comment = new ForumComment
            {
                PostId = postId,
                IssueId = issueId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Depth = 0
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            // Cập nhật CommentCount
            if (postId.HasValue)
            {
                var post = await _context.ForumPosts.FindAsync(postId.Value);
                if (post != null)
                {
                    post.CommentCount++;
                    
                    // Create Notification for Post Author
                    if (post.AuthorId != user.Id)
                    {
                        var notif = new Notification
                        {
                            UserId = post.AuthorId,
                            Title = "Bình luận mới",
                            Message = $"💬 {user.FullName ?? user.UserName} đã bình luận về bài viết của bạn.",
                            Type = NotificationType.General,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notif);
                        await _hubContext.Clients.User(post.AuthorId).SendAsync("ReceiveNotification", notif);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, authorName = user.FullName ?? user.UserName, content = content, createdAt = "Vừa xong" });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReplyComment(int parentCommentId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(content)) return Json(new { success = false });

            var parentComment = await _context.ForumComments.FindAsync(parentCommentId);
            if (parentComment == null || parentComment.Depth >= 3) return Json(new { success = false, message = "Không thể reply sâu hơn 3 tầng." });

            var reply = new ForumComment
            {
                ParentCommentId = parentCommentId,
                PostId = parentComment.PostId,
                IssueId = parentComment.IssueId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Depth = parentComment.Depth + 1
            };

            _context.ForumComments.Add(reply);
            await _context.SaveChangesAsync();

            // Cập nhật CommentCount
            if (parentComment.PostId.HasValue)
            {
                var post = await _context.ForumPosts.FindAsync(parentComment.PostId.Value);
                if (post != null) post.CommentCount++;
            }
            
            // Create Notification for Parent Comment Author
            if (parentComment.AuthorId != user.Id)
            {
                var notif = new Notification
                {
                    UserId = parentComment.AuthorId,
                    Title = "Trả lời bình luận",
                    Message = $"↩️ {user.FullName ?? user.UserName} đã trả lời bình luận của bạn.",
                    Type = NotificationType.General,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notif);
                await _hubContext.Clients.User(parentComment.AuthorId).SendAsync("ReceiveNotification", notif);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, authorName = user.FullName ?? user.UserName, content = content });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            // Thực tế nên có bảng PostLikes để chặn like nhiều lần
            // Tạm thời đơn giản hóa cộng trực tiếp
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null) return Json(new { success = false });

            post.LikeCount++;
            
            // Notification for Like
            if (post.AuthorId != _userManager.GetUserId(User))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var notif = new Notification
                {
                    UserId = post.AuthorId,
                    Title = "Lượt thích mới",
                    Message = $"❤️ {currentUser?.FullName ?? currentUser?.UserName} đã thích bài viết của bạn.",
                    Type = NotificationType.General,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notif);
                await _hubContext.Clients.User(post.AuthorId).SendAsync("ReceiveNotification", notif);
            }
            
            // Tặng 1 điểm TrustScore + 5 CitizenPoints nếu LikeCount >= 10 (chỉ tặng 1 lần thì cần logic cẩn thận hơn, tạm thời call AwardPoints)
            if (post.LikeCount == 10)
            {
                await _pointsService.AwardPointsAsync(post.AuthorId, 5, 1, $"Bài viết '{post.Title}' đạt 10 lượt thích.");
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, likes = post.LikeCount });
        }

        // 3. THĂM DÒ Ý KIẾN (Polls)
        public async Task<IActionResult> Polls()
        {
            var polls = await _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
                
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.UserVotes = await _context.PollVotes
                    .Where(v => v.UserId == userId)
                    .Select(v => v.PollId)
                    .ToListAsync();
            }

            return View(polls);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> VotePoll(int pollId, int optionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingVote = await _context.PollVotes.FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == user.Id);
            if (existingVote != null) return BadRequest("Bạn đã bình chọn rồi.");

            var option = await _context.PollOptions.FindAsync(optionId);
            if (option == null || option.PollId != pollId) return NotFound();

            var vote = new PollVote
            {
                PollId = pollId,
                OptionId = optionId,
                UserId = user.Id
            };
            
            option.VoteCount++;
            _context.PollVotes.Add(vote);
            await _pointsService.AwardPointsAsync(user.Id, 5, 0, $"Tham gia bình chọn: {option.Poll?.Question}");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Polls));
        }

        // 4. KIẾN NGHỊ ĐIỆN TỬ (Petitions)
        public async Task<IActionResult> Petitions()
        {
            var petitions = await _context.Petitions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.UserSignatures = await _context.PetitionSignatures
                    .Where(s => s.UserId == userId)
                    .Select(s => s.PetitionId)
                    .ToListAsync();
            }

            return View(petitions);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SignPetition(int petitionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingSignature = await _context.PetitionSignatures.FirstOrDefaultAsync(s => s.PetitionId == petitionId && s.UserId == user.Id);
            if (existingSignature != null) return BadRequest("Bạn đã ký tên rồi.");

            var petition = await _context.Petitions.FindAsync(petitionId);
            if (petition == null) return NotFound();

            var signature = new PetitionSignature
            {
                PetitionId = petitionId,
                UserId = user.Id
            };

            petition.CurrentSignatures++;
            _context.PetitionSignatures.Add(signature);
            await _pointsService.AwardPointsAsync(user.Id, 5, 0, $"Ký kiến nghị: {petition.Title}");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Petitions));
        }

        // 5. SỰ KIỆN CỘNG ĐỒNG (Events)
        public async Task<IActionResult> Events()
        {
            var events = await _context.CommunityEvents
                .OrderBy(e => e.StartTime)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.UserRegistrations = await _context.EventRegistrations
                    .Where(r => r.UserId == userId)
                    .Select(r => r.EventId)
                    .ToListAsync();
            }

            return View(events);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterEvent(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingReg = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == user.Id);
            if (existingReg != null) return BadRequest("Bạn đã đăng ký rồi.");

            var ev = await _context.CommunityEvents.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();

            if (ev.Registrations.Count >= ev.MaxParticipants) return BadRequest("Sự kiện đã đủ số lượng.");

            var registration = new EventRegistration
            {
                EventId = eventId,
                UserId = user.Id
            };

            _context.EventRegistrations.Add(registration);
            await _pointsService.AwardPointsAsync(user.Id, 10, 0, $"Đăng ký tham gia sự kiện: {ev.Title}");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Events));
        }

        [Authorize(Roles = "Admin,OfficialWard,OfficialDistrict,OfficialProvince,DepartmentStaff")]
        [HttpPost]
        public async Task<IActionResult> CreatePoll(string question, string description, int durationDays, List<string> options)
        {
            if (string.IsNullOrWhiteSpace(question) || options == null || options.Count < 2)
            {
                TempData["ErrorMessage"] = "Thông tin thăm dò ý kiến không hợp lệ. Phải có ít nhất 2 phương án.";
                return RedirectToAction(nameof(Polls));
            }

            var poll = new Poll
            {
                Question = question,
                Description = description ?? "",
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(durationDays > 0 ? durationDays : 7),
                IsActive = true,
                Options = options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => new PollOption { Text = o.Trim(), VoteCount = 0 }).ToList()
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo cuộc thăm dò ý kiến mới thành công!";
            return RedirectToAction(nameof(Polls));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePetition(string title, string description, string targetAudience, int targetSignatures, int durationDays)
        {
            var isOfficial = User.IsInRole("OfficialWard") || User.IsInRole("OfficialDistrict") || User.IsInRole("OfficialProvince") || User.IsInRole("DepartmentStaff");
            if (isOfficial)
            {
                TempData["ErrorMessage"] = "Tài khoản cán bộ không thể tạo kiến nghị công dân.";
                return RedirectToAction(nameof(Petitions));
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || targetSignatures <= 0)
            {
                TempData["ErrorMessage"] = "Thông tin kiến nghị không hợp lệ.";
                return RedirectToAction(nameof(Petitions));
            }

            var petition = new Petition
            {
                Title = title,
                Description = description,
                TargetAudience = targetAudience ?? "Ủy ban Nhân dân",
                TargetSignatures = targetSignatures,
                CurrentSignatures = 0,
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(durationDays > 0 ? durationDays : 30),
                Status = "Active"
            };

            _context.Petitions.Add(petition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo kiến nghị mới thành công!";
            return RedirectToAction(nameof(Petitions));
        }

        [Authorize(Roles = "Admin,OfficialWard,OfficialDistrict,OfficialProvince,DepartmentStaff")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(string title, string description, string location, DateTime startTime, DateTime endTime, string organizer, int maxParticipants)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(location) || startTime >= endTime || maxParticipants <= 0)
            {
                TempData["ErrorMessage"] = "Thông tin sự kiện không hợp lệ.";
                return RedirectToAction(nameof(Events));
            }

            var ev = new CommunityEvent
            {
                Title = title,
                Description = description,
                Location = location,
                StartTime = startTime,
                EndTime = endTime,
                Organizer = organizer ?? "Cổng thông tin Civic Connect",
                ImageUrl = "/images/default-event.jpg",
                MaxParticipants = maxParticipants
            };

            _context.CommunityEvents.Add(ev);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo sự kiện mới thành công!";
            return RedirectToAction(nameof(Events));
        }
    }
}
