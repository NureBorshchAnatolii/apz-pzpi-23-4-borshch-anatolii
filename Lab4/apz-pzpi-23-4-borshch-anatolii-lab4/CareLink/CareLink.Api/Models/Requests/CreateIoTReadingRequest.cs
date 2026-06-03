namespace CareLink.Api.Models.Requests
{
    public class CreateIoTReadingRequest
    {
        public DateTime ReadDateTime { get; set; }
        public long Pulse { get; set; }
        public long ActivityLevel { get; set; }
        public int Temperature { get; set; }
        public string SerialNumber { get; set; } = null!;
    }
}