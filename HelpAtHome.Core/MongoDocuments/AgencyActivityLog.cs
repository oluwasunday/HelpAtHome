using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HelpAtHome.Core.MongoDocuments
{
    public class AgencyActivityLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string AgencyId { get; set; }
        public string AgencyAdminUserId { get; set; }
        public string Action { get; set; }
        public string? CaregiverId { get; set; }
        public string? BookingId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
