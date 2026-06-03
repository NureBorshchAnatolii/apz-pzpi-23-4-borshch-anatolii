namespace CareLink.Application.Dtos.Messages
{
    public record MessageUpdateRequest(
        long MessageId,
        string Content
    );
}