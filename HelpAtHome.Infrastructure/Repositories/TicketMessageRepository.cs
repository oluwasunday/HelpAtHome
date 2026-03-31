using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class TicketMessageRepository : GenericRepository<TicketMessage>, ITicketMessageRepository
    {
        public TicketMessageRepository(AppDbContext context) : base(context) { }
    }
}
