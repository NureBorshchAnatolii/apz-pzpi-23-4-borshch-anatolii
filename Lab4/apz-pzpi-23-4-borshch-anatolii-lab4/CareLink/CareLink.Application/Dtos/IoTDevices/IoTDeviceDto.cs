namespace CareLink.Application.Dtos
{
    public class IoTDeviceDto
    {
        public long Id { get; set; }
        public string SerialNumber { get; set; }
        public long UserId { get; set; }
        public string DeviceType { get; set; }
    }
}