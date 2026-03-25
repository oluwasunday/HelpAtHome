using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HelpAtHome.Core.MongoDocuments
{
    public class NotificationLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Channel { get; set; }    // "Push", "Email", "SMS"
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsSuccess { get; set; }
        public string? FailureReason { get; set; }
        public string? ExternalReference { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
