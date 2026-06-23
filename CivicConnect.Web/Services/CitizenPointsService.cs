using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public interface ICitizenPointsService
    {
        Task AwardPointsAsync(string userId, int citizenPointsDelta, int trustScoreDelta, string reason);
    }

    public class CitizenPointsService : ICitizenPointsService
    {
        private readonly AppDbContext _context;

        public CitizenPointsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AwardPointsAsync(string userId, int citizenPointsDelta, int trustScoreDelta, string reason)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Chỉ cập nhật nếu có thay đổi
            if (citizenPointsDelta != 0 || trustScoreDelta != 0)
            {
                user.CitizenPoints += citizenPointsDelta;
                user.TrustScore += trustScoreDelta;

                // Không để TrustScore dưới 0
                if (user.TrustScore < 0)
                {
                    user.TrustScore = 0;
                }

                var transaction = new PointTransaction
                {
                    UserId = userId,
                    PointsDelta = citizenPointsDelta,
                    TrustScoreDelta = trustScoreDelta,
                    Reason = reason
                };

                _context.PointTransactions.Add(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
