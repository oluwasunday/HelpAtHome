namespace HelpAtHome.Core.DTOs.Requests
{
    public class EmailRequest
    {
        public string HtmlContent { get; set; }
        public Sender Sender { get; set; }
        public string Subject { get; set; }
        public List<Recipient> To { get; set; }
    }

    public class Sender
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class Recipient
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

}
