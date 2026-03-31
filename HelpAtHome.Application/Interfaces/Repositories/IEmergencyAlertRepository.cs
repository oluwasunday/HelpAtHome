using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IEmergencyAlertRepository : IGenericRepository<EmergencyAlert>
    {
        Task<EmergencyAlert?> GetWithDetailsAsync(Guid alertId);
        Task<(List<EmergencyAlert> Items, int Total)> GetPagedAsync(
            Guid? clientProfileId, AlertStatus? status, int page, int size);
    }
}
