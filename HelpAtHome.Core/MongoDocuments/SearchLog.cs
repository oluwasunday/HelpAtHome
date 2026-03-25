using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HelpAtHome.Core.MongoDocuments
{
    public class SearchLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? UserId { get; set; }
        public string? SearchTerm { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? ServiceCategory { get; set; }
        public int ResultsCount { get; set; }
        public string? IpAddress { get; set; }
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}
