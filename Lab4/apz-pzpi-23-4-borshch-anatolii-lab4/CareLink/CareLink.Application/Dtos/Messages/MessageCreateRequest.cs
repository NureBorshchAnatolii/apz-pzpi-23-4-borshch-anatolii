namespace CareLink.Application.Dtos.Messages
{
    public record MessageCreateRequest(
        string Content,
        long SenderId,
        long ReceiverId
    );
}