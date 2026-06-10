using CivicConnect.Core.Entities;
using CivicConnect.Core.Enums;
using CivicConnect.Core.Interfaces;
using CivicConnect.Web.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class IssuesController : Controller
    {
        private readonly IIssueService _issueService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly CivicConnect.Infrastructure.Data.AppDbContext _context;

        public IssuesController(
            IIssueService issueService,
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService,
            CivicConnect.Infrastructure.Data.AppDbContext context)
        {
            _issueService = issueService;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(IssueCategory? category, IssueStatus? status, string? search, string? sort)
        {
            ViewData["PageHeader"] = "Danh Sách Phản Ánh";

            var query = _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .AsQueryable();

            // Lọc theo danh mục
            if (category.HasValue)
            {
                query = query.Where(i => i.Category == category.Value);
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            // Tìm kiếm theo tiêu đề/mô tả
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Title.Contains(search) || i.Description.Contains(search) || i.Address.Contains(search));
            }

            // Sắp xếp
            query = sort switch
            {
                "priority" => query.OrderByDescending(i => i.PriorityScore),
                "oldest" => query.OrderBy(i => i.CreatedAt),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            var issues = await query.ToListAsync();

            ViewData["CurrentCategory"] = category;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentSort"] = sort;

            return View(issues);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var issue = await _issueService.GetIssueByIdAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            // Tăng lượt xem
            issue.ViewCount++;
            _context.Entry(issue).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            ViewData["PageHeader"] = "Chi Tiết Phản Ánh";
            return View(issue);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewData["PageHeader"] = "Gửi Phản Ánh Mới";
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) return Challenge();

                var issue = new Issue
                {
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    Priority = model.Priority,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Address = model.Address,
                    WardCode = model.WardCode ?? "26734",
                    WardName = model.WardName ?? "Phường Bến Nghé",
                    DistrictCode = model.DistrictCode ?? "760",
                    DistrictName = model.DistrictName ?? "Quận 1",
                    ProvinceCode = model.ProvinceCode ?? "79",
                    ProvinceName = model.ProvinceName ?? "TP. Hồ Chí Minh",
                    AuthorId = userId,
                    IsAnonymous = model.IsAnonymous,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Tải lên tối đa 5 hình ảnh
                var images = new List<IssueImage>();
                if (model.ImageFiles != null && model.ImageFiles.Count > 0)
                {
                    if (model.ImageFiles.Count > 5)
                    {
                        ModelState.AddModelError(nameof(model.ImageFiles), "Chỉ cho phép tải lên tối đa 5 hình ảnh.");
                        ViewData["PageHeader"] = "Gửi Phản Ánh Mới";
                        return View(model);
                    }

                    // Khởi tạo trước Issue để lấy ID cho thư mục Cloudinary
                    var createdIssue = await _issueService.CreateIssueAsync(issue, new List<IssueImage>());

                    foreach (var file in model.ImageFiles)
                    {
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError(nameof(model.ImageFiles), $"Tệp {file.FileName} vượt quá kích thước cho phép (5MB).");
                            ViewData["PageHeader"] = "Gửi Phản Ánh Mới";
                            return View(model);
                        }

                        using (var stream = file.OpenReadStream())
                        {
                            var upload = await _cloudinaryService.UploadIssueImageAsync(
                                stream,
                                file.FileName,
                                file.ContentType,
                                createdIssue.Id
                            );

                            images.Add(new IssueImage
                            {
                                IssueId = createdIssue.Id,
                                PublicId = upload.PublicId,
                                Url = upload.Url,
                                ThumbnailUrl = upload.ThumbnailUrl,
                                OriginalFileName = file.FileName,
                                FileSizeBytes = file.Length,
                                UploadedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Lưu cập nhật ảnh
                    for (int i = 0; i < images.Count; i++)
                    {
                        images[i].OrderIndex = i;
                        await _context.IssueImages.AddAsync(images[i]);
                    }
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Details), new { id = createdIssue.Id });
                }
                else
                {
                    var createdIssue = await _issueService.CreateIssueAsync(issue, images);
                    return RedirectToAction(nameof(Details), new { id = createdIssue.Id });
                }
            }

            ViewData["PageHeader"] = "Gửi Phản Ánh Mới";
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyIssues()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var issues = await _issueService.GetIssuesForUserAsync(userId);
            ViewData["PageHeader"] = "Phản Ánh Của Tôi";
            return View(issues);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Vote(int id, VoteType type)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _issueService.VoteIssueAsync(id, userId, type);
            if (!success) return BadRequest();

            var issue = await _issueService.GetIssueByIdAsync(id);
            if (issue == null) return NotFound();

            var upvotes = issue.Votes.Count(v => v.Type == VoteType.Up);
            var downvotes = issue.Votes.Count(v => v.Type == VoteType.Down);

            return Json(new { success = true, upvotes, downvotes });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comment(int id, string content, int? parentCommentId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content)) return BadRequest("Nội dung bình luận không được để trống.");

            var isOfficial = User.IsInRole("OfficialWard") || User.IsInRole("OfficialDistrict") || User.IsInRole("OfficialProvince") || User.IsInRole("DepartmentStaff");

            var comment = await _issueService.AddCommentAsync(id, userId, content, parentCommentId, isOfficial);

            return Json(new { 
                success = true, 
                id = comment.Id,
                authorName = (await _userManager.FindByIdAsync(comment.AuthorId))?.FullName ?? "Nặc danh",
                createdAt = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                content = comment.Content,
                isOfficial = comment.IsOfficialResponse
            });
        }
    }
}
