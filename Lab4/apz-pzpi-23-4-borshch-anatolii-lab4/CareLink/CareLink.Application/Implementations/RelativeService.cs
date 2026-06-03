using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Relatives;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class RelativeService : IRelativeService
    {
        private readonly IRelativeRepository _relativeRepository;
        private readonly IUserRepository _userRepository;


        public RelativeService(IRelativeRepository relativeRepository, IUserRepository userRepository)
        {
            _relativeRepository = relativeRepository;
            _userRepository = userRepository;
        }
        
        public async Task<IEnumerable<RelativeDto>> GetAllRelativesAsync(long currentUserId)
        {
            var relatives = await _relativeRepository.GetAllIncludedRelatives();

            // Return relations where the current user is either the guardian
            // or the relative, so that contacts are visible bidirectionally.
            var userRelations = relatives.Where(r =>
                r.GuardianUserId == currentUserId || r.RelativeUserId == currentUserId);

            return userRelations.Select(r => MapToRelativeDto(r, currentUserId));
        }

        public async Task CreateRelativeAsync(RelativeCreateCommand request)
        {
            await ValidateUsersForCreate(request);

            await EnsureRelativeNotAssigned(request);

            var entity = new Relatives
            {
                GuardianUserId = request.GuardianUserId,
                RelativeUserId = request.RelativeUserId,
                RelationTypeId = request.RelationTypeId,
                AddedAt = DateTime.UtcNow
            };

            await _relativeRepository.AddAsync(entity);
        }

        public async Task DeleteRelativeAsync(RelativeDeleteCommand request)
        {
            var relatives = await _relativeRepository.GetAllIncludedRelatives();
            var relative = relatives.FirstOrDefault(x => x.Id == request.RelativeId)
                           ?? throw new ArgumentException("Relative user not found");;

            EnsureGuardianOwnership(request.GuardianUserId, relative);

            await _relativeRepository.DeleteAsync(relative);
        }

        private async Task ValidateUsersForCreate(RelativeCreateCommand request)
        {
            var relativeUser = await _userRepository.GetByIdAsync(request.RelativeUserId)
                               ?? throw new ArgumentException("Relative user not found");

            _ = await _userRepository.GetByIdAsync(request.GuardianUserId)
                ?? throw new ArgumentException("Guardian user not found");

            if (relativeUser.RoleId != 5)
                throw new InvalidOperationException("User does not have the 'Relative' role");
        }

        private async Task EnsureRelativeNotAssigned(RelativeCreateCommand request)
        {
            var existing = await _relativeRepository.ExistItemAsync(r =>
                r.GuardianUserId == request.GuardianUserId &&
                r.RelativeUserId == request.RelativeUserId);

            if (existing)
                throw new InvalidOperationException("This relative is already assigned to the guardian");
        }

        private void EnsureGuardianOwnership(long currentUserId, Relatives relative)
        {
            // Either party of the relation may remove it.
            if (relative.GuardianUserId != currentUserId && relative.RelativeUserId != currentUserId)
                throw new UnauthorizedAccessException("User is not part of this relation");
        }

        private RelativeDto MapToRelativeDto(Relatives relative, long currentUserId)
        {
            // From the current user's perspective, expose the *other* party
            // through the RelativeId/RelativeFullName fields so the same DTO
            // can be consumed by both guardians and relatives.
            var viewerIsGuardian = relative.GuardianUserId == currentUserId;

            var otherUser = viewerIsGuardian ? relative.RelativeUser : relative.GuardianUser;
            var otherUserId = viewerIsGuardian ? relative.RelativeUserId : relative.GuardianUserId;
            var selfUser = viewerIsGuardian ? relative.GuardianUser : relative.RelativeUser;
            var selfUserId = viewerIsGuardian ? relative.GuardianUserId : relative.RelativeUserId;

            return new RelativeDto()
            {
                Id = relative.Id,
                RelationType = relative.RelationType.Name,
                AddedAt = relative.AddedAt,
                GuardianId = selfUserId,
                GuardianFullName = selfUser.FirstName + " " + selfUser.LastName,
                RelativeId = otherUserId,
                RelativeFullName = otherUser.FirstName + " " + otherUser.LastName
            };
        }
    }
}