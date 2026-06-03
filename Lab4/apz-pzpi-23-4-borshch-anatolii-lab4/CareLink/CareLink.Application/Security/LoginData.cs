namespace CareLink.Application.Security
{
    public class LoginData
    {
        public long UserId { get; set; }
        public string Token { get; set; } = null!;
    }
}