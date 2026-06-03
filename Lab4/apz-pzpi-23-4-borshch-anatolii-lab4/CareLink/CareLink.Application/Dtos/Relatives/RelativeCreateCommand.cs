namespace CareLink.Application.Dtos.Relatives
{
    public record RelativeCreateCommand(long GuardianUserId, long RelativeUserId, long RelationTypeId);
}