namespace CareLink.Domain.Entities
{
    public class IoTReading : BaseEntity
    {
        public DateTime ReadDateTime { get; set; }
        public long Pulse { get; set; }
        public long ActivityLevel { get; set; }
        public int Temperature { get; set; }
        
        public long DeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; } = null!;
    }
}