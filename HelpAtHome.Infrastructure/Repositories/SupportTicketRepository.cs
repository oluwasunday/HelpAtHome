using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class SupportTicketRepository : GenericRepository<SupportTicket>, ISupportTicketRepository
    {
        private readonly AppDbContext _ctx;

        public SupportTicketRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<SupportTicket?> GetWithDetailsAsync(Guid ticketId)
        {
            return await _ctx.SupportTickets
                .Include(t => t.RaisedBy)
                .Include(t => t.Booking)
                .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<(List<SupportTicket> Items, int Total)> GetPagedAsync(
            Guid? userId, TicketStatus? status, TicketPriority? priority, int page, int size)
        {
            var query = _ctx.SupportTickets
                .Include(t => t.RaisedBy)
                .Include(t => t.Booking)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(t => t.RaisedByUserId == userId.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            query = query.OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, total);
        }
    }
}
