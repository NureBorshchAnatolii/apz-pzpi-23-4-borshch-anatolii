namespace CareLink.Application.Contracts.Repositories
{
    public interface IPdfService
    {
        Task<byte[]> GenerateRelativeReportAsync(long guardianUserId, long relativeUserId);
    }
}