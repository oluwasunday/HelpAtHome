using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IEmergencyService
    {
        Task<Result<EmergencyAlertDto>> TriggerAlertAsync(Guid clientUserId, TriggerAlertDto dto);
        Task<Result<EmergencyAlertDto>> GetAlertAsync(Guid userId, Guid alertId);
        Task<Result<PagedResult<EmergencyAlertDto>>> GetMyAlertsAsync(Guid clientUserId, int page, int size);
        Task<Result<PagedResult<EmergencyAlertDto>>> GetActiveAlertsAsync(AlertStatus? status, int page, int size);
        Task<Result<EmergencyAlertDto>> RespondToAlertAsync(Guid responderId, Guid alertId, RespondAlertDto dto);
    }
}
