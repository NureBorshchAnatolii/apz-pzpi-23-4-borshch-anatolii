namespace CareLink.Application.Dtos.Notiffications
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public string GroupOfIds { get; set; } = null!;
        public long NotificationTypeId { get; set; }
    }
}