namespace CareLink.Application.Contracts.Services
{
    public interface IRelativeMetricsService
    {
        Task<bool> IsRelativeOfAsync(long guardianUserId, long relativeUserId);
        Task<double> CalculateCognitiveReserveAsync(long guardianUserId, long relativeUserId);
        Task<double> CalculatePhysicalActivityDeclineAsync(long guardianUserId, long relativeUserId);
        Task<double> CalculateRestingHeartRateAsync(long guardianUserId, long relativeUserId);
        Task<double> CalculateHeartRateVariabilityAsync(long guardianUserId, long relativeUserId);
        Task<double> CalculateSocialIsolationIndexAsync(long guardianUserId, long relativeUserId);
    }
}