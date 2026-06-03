namespace CareLink.Application.Dtos.Admin
{
    public class SystemStateDto
    {
        public int TotalUsers { get; set; }
        public int IoTDevices { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int NotificationsLast24h { get; set; }
    }
}