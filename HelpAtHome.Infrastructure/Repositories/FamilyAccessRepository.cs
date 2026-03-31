using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class FamilyAccessRepository : GenericRepository<FamilyAccess>, IFamilyAccessRepository
    {
        private readonly AppDbContext _ctx;

        public FamilyAccessRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<FamilyAccess?> GetWithUsersAsync(Guid accessId)
        {
            return await _ctx.FamilyAccesses
                .Include(f => f.Client)
                .Include(f => f.FamilyMember)
                .FirstOrDefaultAsync(f => f.Id == accessId);
        }

        public async Task<List<FamilyAccess>> GetByClientUserIdAsync(Guid clientUserId)
        {
            return await _ctx.FamilyAccesses
                .Include(f => f.Client)
                .Include(f => f.FamilyMember)
                .Where(f => f.ClientUserId == clientUserId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FamilyAccess>> GetByFamilyMemberUserIdAsync(Guid familyMemberUserId)
        {
            return await _ctx.FamilyAccesses
                .Include(f => f.Client)
                .Include(f => f.FamilyMember)
                .Where(f => f.FamilyMemberUserId == familyMemberUserId && f.IsApproved)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<FamilyAccess?> GetByPairAsync(Guid clientUserId, Guid familyMemberUserId)
        {
            return await _ctx.FamilyAccesses
                .FirstOrDefaultAsync(f =>
                    f.ClientUserId == clientUserId &&
                    f.FamilyMemberUserId == familyMemberUserId);
        }
    }
}
