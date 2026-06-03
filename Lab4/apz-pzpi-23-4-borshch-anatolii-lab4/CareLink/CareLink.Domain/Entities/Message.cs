namespace CareLink.Domain.Entities
{
    public class Message : BaseEntity
    {
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        
        public long SenderId { get; set; }
        public User Sender { get; set; } = null!;
        
        public long ReceiverId { get; set; }
        public User Receiver { get; set; } =  null!;
    }
}