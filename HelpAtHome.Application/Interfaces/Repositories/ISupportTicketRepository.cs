using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface ISupportTicketRepository : IGenericRepository<SupportTicket>
    {
        Task<SupportTicket?> GetWithDetailsAsync(Guid ticketId);
        Task<(List<SupportTicket> Items, int Total)> GetPagedAsync(
            Guid? userId, TicketStatus? status, TicketPriority? priority, int page, int size);
    }
}
