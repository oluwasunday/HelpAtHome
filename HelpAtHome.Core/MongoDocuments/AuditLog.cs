using HelpAtHome.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HelpAtHome.Core.MongoDocuments
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PerformedByUserId { get; set; }
        public string PerformedByRole { get; set; }
        public AuditAction Action { get; set; }
        public string EntityName { get; set; }    // e.g., "Booking", "User"
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }    // JSON before
        public string? NewValues { get; set; }    // JSON after
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
