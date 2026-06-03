namespace CareLink.Application.Dtos.Admin
{
    public class SystemLogDto
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Source { get; set; }
    }
}