using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class EmergencyAlertRepository : GenericRepository<EmergencyAlert>, IEmergencyAlertRepository
    {
        private readonly AppDbContext _ctx;
        public EmergencyAlertRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
