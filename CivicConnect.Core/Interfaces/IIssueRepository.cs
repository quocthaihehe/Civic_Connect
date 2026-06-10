using CivicConnect.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface IIssueRepository
    {
        Task<Issue?> GetByIdAsync(int id);
        Task<IEnumerable<Issue>> GetAllAsync();
        Task<IEnumerable<Issue>> GetByWardAsync(string wardCode);
        Task<IEnumerable<Issue>> GetByDistrictAsync(string districtCode);
        Task<IEnumerable<Issue>> GetPublicAsync();
        Task AddAsync(Issue issue);
        Task UpdateAsync(Issue issue);
        Task DeleteAsync(Issue issue);
        Task SaveChangesAsync();
    }
}
