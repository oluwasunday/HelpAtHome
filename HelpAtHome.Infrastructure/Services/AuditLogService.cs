using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using HelpAtHome.Core.MongoDocuments;
using HelpAtHome.Infrastructure.MongoDB;
using HelpAtHome.Shared;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HelpAtHome.Infrastructure.Audit
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;
        private readonly IMongoCollection<AgencyActivityLog> _agencyActivity;

        public AuditLogService(MongoDbContext mongo, IOptions<MongoDbSettings> settings)
        {
            _auditLogs = mongo.GetCollection<AuditLog>(settings.Value.AuditLogsCollection);
            _agencyActivity = mongo.GetCollection<AgencyActivityLog>(settings.Value.AgencyActivityCollection);
        }

        public async Task LogAsync(
            Guid userId,
            string role,
            AuditAction action,
            string entityName,
            string? entityId = null,
            string? notes = null,
            string? ipAddress = null)
        {
            var log = new AuditLog
            {
                PerformedByUserId = userId.ToString(),
                PerformedByRole = role,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Notes = notes,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogs.InsertOneAsync(log);
        }

        public async Task LogAgencyActivityAsync(
            Guid agencyId,
            Guid agencyAdminUserId,
            string action,
            string? caregiverId = null,
            string? bookingId = null,
            string? details = null)
        {
            var log = new AgencyActivityLog
            {
                AgencyId = agencyId.ToString(),
                AgencyAdminUserId = agencyAdminUserId.ToString(),
                Action = action,
                CaregiverId = caregiverId,
                BookingId = bookingId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await _agencyActivity.InsertOneAsync(log);
        }

        public async Task<Result<PagedResult<AuditLogDto>>> QueryAsync(AuditLogFilterDto filter)
        {
            var builder = Builders<AuditLog>.Filter;
            var filterDef = builder.Empty;

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                filterDef &= builder.Eq(x => x.PerformedByUserId, filter.UserId);

            if (filter.Action.HasValue)
                filterDef &= builder.Eq(x => x.Action, filter.Action.Value);

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
                filterDef &= builder.Eq(x => x.EntityName, filter.EntityName);

            if (filter.From.HasValue)
                filterDef &= builder.Gte(x => x.CreatedAt, filter.From.Value);

            if (filter.To.HasValue)
                filterDef &= builder.Lte(x => x.CreatedAt, filter.To.Value);

            var total = (int)await _auditLogs.CountDocumentsAsync(filterDef);

            var items = await _auditLogs
                .Find(filterDef)
                .SortByDescending(x => x.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            var dtos = items.Select(x => new AuditLogDto
            {
                Id = x.Id,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByRole = x.PerformedByRole,
                Action = x.Action.ToString(),
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IpAddress = x.IpAddress,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            }).ToList();

            return Result<PagedResult<AuditLogDto>>.Ok(
                new PagedResult<AuditLogDto>(dtos, total, filter.Page, filter.PageSize));
        }

        public async Task<Result<PagedResult<AgencyActivityLogDto>>> GetAgencyActivityAsync(
            Guid agencyId, int page, int size)
        {
            var filter = Builders<AgencyActivityLog>.Filter
                .Eq(x => x.AgencyId, agencyId.ToString());

            var total = (int)await _agencyActivity.CountDocumentsAsync(filter);

            var items = await _agencyActivity
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();

            var dtos = items.Select(x => new AgencyActivityLogDto
            {
                Id = x.Id,
                AgencyId = x.AgencyId,
                AgencyAdminUserId = x.AgencyAdminUserId,
                Action = x.Action,
                CaregiverId = x.CaregiverId,
                BookingId = x.BookingId,
                Details = x.Details,
                CreatedAt = x.CreatedAt
            }).ToList();

            return Result<PagedResult<AgencyActivityLogDto>>.Ok(
                new PagedResult<AgencyActivityLogDto>(dtos, total, page, size));
        }
    }
}
