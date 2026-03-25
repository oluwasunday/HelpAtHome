namespace HelpAtHome.Infrastructure.MongoDB
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string AuditLogsCollection { get; set; }
        public string AgencyActivityCollection { get; set; }
        public string NotificationLogsCollection { get; set; }
        public string SearchLogsCollection { get; set; }
    }
}
