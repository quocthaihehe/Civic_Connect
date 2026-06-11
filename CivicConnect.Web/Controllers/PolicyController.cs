using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Infrastructure.Data;
using CivicConnect.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System;

namespace CivicConnect.Web.Controllers
{
    public class PolicyController : Controller
    {
        private readonly AppDbContext _context;

        public PolicyController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageHeader"] = "Chính Sách & Tin Tức";
            ViewData["Title"] = "Chính Sách & Tin Tức";
            
            // Load policies from database
            var dbPolicies = await _context.Policies
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            // Fetch dynamic RSS news
            var rssNews = await FetchRssNewsAsync();

            // Combine list (DB policies first, then RSS news, both ordered by PublishedDate)
            var allPolicies = dbPolicies.Concat(rssNews)
                .OrderByDescending(p => p.PublishedDate)
                .ToList();

            return View(allPolicies);
        }

        public async Task<IActionResult> Details(int id, string url)
        {
            if (id == 0 && !string.IsNullOrEmpty(url))
            {
                // Create dynamic Policy model for external RSS link
                var policy = new Policy
                {
                    Id = 0,
                    Title = "Tin Tức Trực Tuyến",
                    Content = "Bạn đang xem nội dung được liên kết trực tiếp từ cổng thông tin báo chí hoặc chính sách nhà nước.",
                    SourceUrl = url,
                    Tag = "Tin tức",
                    TagClass = "tag-news",
                    IssuingUnit = "Báo điện tử",
                    PublishedDate = DateTime.UtcNow,
                    IsActive = true
                };

                ViewData["PageHeader"] = policy.Title;
                ViewData["Title"] = policy.Title;
                return View(policy);
            }

            var policyDb = await _context.Policies.FindAsync(id);
            if (policyDb == null || !policyDb.IsActive)
            {
                return NotFound();
            }

            ViewData["PageHeader"] = policyDb.Title;
            ViewData["Title"] = policyDb.Title;
            return View(policyDb);
        }

        private async Task<List<Policy>> FetchRssNewsAsync()
        {
            var newsList = new List<Policy>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(2500);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    // Fetch live RSS feed from VnExpress
                    var xmlContent = await client.GetStringAsync("https://vnexpress.net/rss/tin-moi-nhat.rss");

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlContent);

                    var items = xmlDoc.SelectNodes("//item");
                    if (items != null)
                    {
                        int count = 0;
                        foreach (XmlNode item in items)
                        {
                            if (count >= 5) break;

                            var title = item.SelectSingleNode("title")?.InnerText ?? "";
                            var link = item.SelectSingleNode("link")?.InnerText ?? "";
                            var description = item.SelectSingleNode("description")?.InnerText ?? "";
                            var pubDateStr = item.SelectSingleNode("pubDate")?.InnerText ?? "";

                            DateTime pubDate = DateTime.UtcNow;
                            if (DateTime.TryParse(pubDateStr, out var parsedDate))
                            {
                                pubDate = parsedDate;
                            }

                            // Clean description by removing HTML tags
                            var cleanDescription = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty).Trim();

                            newsList.Add(new Policy
                            {
                                Id = 0,
                                Title = title,
                                Excerpt = cleanDescription,
                                Content = "Nội dung bài viết được tải trực tiếp bên dưới.",
                                Tag = "Tin tức",
                                TagClass = "tag-news",
                                IssuingUnit = "Báo điện tử",
                                PublishedDate = pubDate,
                                IsActive = true,
                                SourceUrl = link
                            });

                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RSS Fetch Error: {ex.Message}");
            }
            return newsList;
        }
    }
}
