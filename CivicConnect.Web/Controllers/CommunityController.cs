using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class CommunityController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommunityController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. MẠNG XÃ HỘI HUB (Index)
        public async Task<IActionResult> Index()
        {
            var topPosts = await _context.ForumPosts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Upvotes)
                .Take(5)
                .ToListAsync();

            var activePolls = await _context.Polls
                .Where(p => p.IsActive && p.EndDate > DateTime.UtcNow)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            var upcomingEvents = await _context.CommunityEvents
                .Where(e => e.StartTime > DateTime.UtcNow)
                .OrderBy(e => e.StartTime)
                .Take(3)
                .ToListAsync();

            ViewBag.TopPosts = topPosts;
            ViewBag.ActivePolls = activePolls;
            ViewBag.UpcomingEvents = upcomingEvents;

            return View();
        }

        // 2. DIỄN ĐÀN (Forum)
        public async Task<IActionResult> Forum()
        {
            var posts = await _context.ForumPosts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
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
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Forum));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(content)) return BadRequest();

            var comment = new ForumComment
            {
                PostId = postId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Forum));
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
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Events));
        }
    }
}
