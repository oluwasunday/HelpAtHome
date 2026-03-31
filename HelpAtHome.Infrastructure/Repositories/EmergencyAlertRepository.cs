using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class EmergencyAlertRepository : GenericRepository<EmergencyAlert>, IEmergencyAlertRepository
    {
        private readonly AppDbContext _ctx;

        public EmergencyAlertRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<EmergencyAlert?> GetWithDetailsAsync(Guid alertId)
        {
            return await _ctx.EmergencyAlerts
                .Include(a => a.ClientProfile)
                    .ThenInclude(c => c.User)
                .Include(a => a.ActiveBooking)
                .FirstOrDefaultAsync(a => a.Id == alertId);
        }

        public async Task<(List<EmergencyAlert> Items, int Total)> GetPagedAsync(
            Guid? clientProfileId, AlertStatus? status, int page, int size)
        {
            var query = _ctx.EmergencyAlerts
                .Include(a => a.ClientProfile)
                    .ThenInclude(c => c.User)
                .Include(a => a.ActiveBooking)
                .AsQueryable();

            if (clientProfileId.HasValue)
                query = query.Where(a => a.ClientProfileId == clientProfileId.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            query = query.OrderByDescending(a => a.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, total);
        }
    }
}
