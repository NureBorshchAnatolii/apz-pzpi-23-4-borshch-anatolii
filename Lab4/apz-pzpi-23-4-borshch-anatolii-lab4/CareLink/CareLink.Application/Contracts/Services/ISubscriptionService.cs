namespace CareLink.Application.Contracts.Services
{
    public interface ISubscriptionService
    {
        Task<bool> HasAccessAsync(long userId, string featureKey);
    }
}