namespace CareLink.Application.Dtos.IoTDevices
{
    public record IoTDeviceCreateRequest(long UserId, string SerialNumber, long DeviceTypeId);
}