namespace CareLink.Api.Models.Requests
{
    public class UpdateIoTDeviceRequest
    {
        public string SerialNumber { get; set; }
        public long DeviceTypeId { get; set; }
    }
}