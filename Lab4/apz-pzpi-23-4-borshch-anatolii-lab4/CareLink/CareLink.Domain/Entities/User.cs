using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime DateOdBirth { get; set; }
        public DateTime DateCreated { get; set; }
        public string Address { get; set; } = null!;
        
        public long RoleId { get; set; }
        public Role Role { get; set; } = null!;
        
        public ICollection<Relatives> RelativeOf { get; set; } = new List<Relatives>();
        public ICollection<Relatives> Guardians { get; set; } = new List<Relatives>();

        public ICollection<CognitiveExercise> CognitiveExercises { get; set; } = new List<CognitiveExercise>();
        public ICollection<IoTDevice> IoTDevices { get; set; } = new List<IoTDevice>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}