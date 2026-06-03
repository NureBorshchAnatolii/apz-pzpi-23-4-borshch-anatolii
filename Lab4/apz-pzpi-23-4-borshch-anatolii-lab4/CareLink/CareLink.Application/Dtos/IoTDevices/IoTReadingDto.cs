namespace CareLink.Application.Dtos.IoTDevices
{
    public class IoTReadingDto
    {
        public long Id { get; set; }
        public DateTime ReadDateTime { get; set; }
        public long Pulse { get; set; }
        public long ActivityLevel { get; set; }
        public int Temperature { get; set; }
    }
}