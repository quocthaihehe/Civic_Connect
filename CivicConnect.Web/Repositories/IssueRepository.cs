using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Repositories
{
    public class IssueRepository : IIssueRepository
    {
        private readonly AppDbContext _context;

        public IssueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Issue?> GetByIdAsync(int id)
        {
            return await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.AssignedTo)
                .Include(i => i.AssignedUnit)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .Include(i => i.StatusHistory)
                    .ThenInclude(sh => sh.ChangedBy)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Author)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Issue>> GetAllAsync()
        {
            return await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetByWardAsync(string wardCode)
        {
            return await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .Where(i => i.WardCode == wardCode)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetByDistrictAsync(string districtCode)
        {
            return await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .Where(i => i.DistrictCode == districtCode)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetPublicAsync()
        {
            return await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Issue issue)
        {
            await _context.Issues.AddAsync(issue);
        }

        public async Task UpdateAsync(Issue issue)
        {
            _context.Entry(issue).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Issue issue)
        {
            _context.Issues.Remove(issue);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
