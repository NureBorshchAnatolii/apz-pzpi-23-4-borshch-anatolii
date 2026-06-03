namespace CareLink.Application.Dtos.IoTDevices
{
    public record IoTDeviceUpdateRequest(long UserId, long DeviceId, string SerialNumber, long DeviceTypeId);
}