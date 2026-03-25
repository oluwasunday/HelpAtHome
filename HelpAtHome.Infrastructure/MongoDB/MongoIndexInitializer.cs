using HelpAtHome.Core.MongoDocuments;
using MongoDB.Driver;

namespace HelpAtHome.Infrastructure.MongoDB
{
    public class MongoIndexInitializer
    {
        public static async Task InitializeAsync(MongoDbContext ctx, MongoDbSettings settings)
        {
            var auditColl = ctx.GetCollection<AuditLog>(settings.AuditLogsCollection);
            await auditColl.Indexes.CreateOneAsync(
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Ascending(x => x.PerformedByUserId)
                        .Ascending(x => x.CreatedAt)));
            await auditColl.Indexes.CreateOneAsync(
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Ascending(x => x.EntityName)
                        .Ascending(x => x.EntityId)));
            // TTL index: auto-delete audit logs older than 2 years
            await auditColl.Indexes.CreateOneAsync(
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Ascending(x => x.CreatedAt),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(730) }));
        }
    }
}
