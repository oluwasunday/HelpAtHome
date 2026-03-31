using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        /// <summary>Record a general platform audit event to MongoDB.</summary>
        Task LogAsync(
            Guid userId,
            string role,
            AuditAction action,
            string entityName,
            string? entityId = null,
            string? notes = null,
            string? ipAddress = null);

        /// <summary>Record an agency-specific activity event to MongoDB.</summary>
        Task LogAgencyActivityAsync(
            Guid agencyId,
            Guid agencyAdminUserId,
            string action,
            string? caregiverId = null,
            string? bookingId = null,
            string? details = null);

        /// <summary>Query audit logs with optional filters. Admin only.</summary>
        Task<Result<PagedResult<AuditLogDto>>> QueryAsync(AuditLogFilterDto filter);

        /// <summary>Get paginated activity log for a specific agency.</summary>
        Task<Result<PagedResult<AgencyActivityLogDto>>> GetAgencyActivityAsync(Guid agencyId, int page, int size);
    }
}
