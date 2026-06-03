namespace CareLink.Application.Dtos.Messages
{
    public class MessageDto
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
    }
}