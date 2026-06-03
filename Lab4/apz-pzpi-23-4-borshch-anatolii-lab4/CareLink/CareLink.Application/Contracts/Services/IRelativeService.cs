using CareLink.Application.Dtos.Relatives;

namespace CareLink.Application.Contracts.Services
{
    public interface IRelativeService
    {
        Task<IEnumerable<RelativeDto>> GetAllRelativesAsync(long guardianUserId);
        Task CreateRelativeAsync(RelativeCreateCommand request);
        Task DeleteRelativeAsync(RelativeDeleteCommand request);
    }
}