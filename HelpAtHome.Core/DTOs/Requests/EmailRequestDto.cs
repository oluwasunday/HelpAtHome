namespace HelpAtHome.Core.DTOs.Requests
{
    public class EmailRequestDto
    {
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string CcEmail { get; set; }
        public string CcName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
