using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class IoTDevice : BaseEntity
    {
        public string SerialNumber { get; set; } = null!;
        
        public bool IsActive { get; set; }
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        
        public long DeviceTypeId { get; set; }
        public DeviceType DeviceType { get; set; } = null!;
        
        public ICollection<IoTReading> Readings { get; set; } = new List<IoTReading>();
    }
}