namespace CareLink.Api.Models.Requests
{
    public class CreateMessageRequest
    {
        public string Content { get; set; }
        public long ReceiverId { get; set; }
    }
}