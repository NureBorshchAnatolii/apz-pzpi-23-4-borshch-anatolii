namespace CareLink.Api.Models.Requests
{
    public class CreateIoTDeviceRequest
    {
        public string SerialNumber { get; set; }
        public long DeviceTypeId { get; set; }
    }
}