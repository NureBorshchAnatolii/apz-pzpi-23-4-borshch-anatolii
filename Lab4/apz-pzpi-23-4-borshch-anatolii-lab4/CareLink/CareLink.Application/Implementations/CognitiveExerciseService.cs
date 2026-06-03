using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.CognitiveExercise;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class CognitiveExerciseService : ICognitiveExerciseService
    {
        private readonly ICognitiveExerciseRepository _cognitiveRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICognitiveExerciseTypeRepository _cognitiveExerciseTypeRepository;
        private readonly IDifficultyRepository _difficultyRepository;
        private readonly ICognitiveResultRepository _resultRepository;

        public CognitiveExerciseService(
            ICognitiveExerciseRepository cognitiveRepository, 
            IUserRepository userRepository,
            ICognitiveExerciseTypeRepository cognitiveExerciseTypeRepository,
            IDifficultyRepository difficultyRepository,
            ICognitiveResultRepository resultRepository)
        {
            _cognitiveRepository = cognitiveRepository;
            _userRepository = userRepository;
            _cognitiveExerciseTypeRepository = cognitiveExerciseTypeRepository;
            _difficultyRepository = difficultyRepository;
            _resultRepository = resultRepository;
        }

        public async Task<IEnumerable<CognitiveExerciseDto>> GetAllCognitiveExercisesAsync()
        {
            var cognitiveExercises = await _cognitiveRepository.GetAllIncludedAsync();
            var mapped = cognitiveExercises.Select(MapToCognitiveExerciseDto);
            return mapped;
        }

        public async Task CreateCognitiveExercise(CognitiveExerciseCreateRequest request)
        {
            await ValidateCreateRequest(request);

            var exercise = new CognitiveExercise()
            {
                UserId = request.UserId,
                Title = request.Title,
                Description = request.Description,
                DifficultyId = request.DifficultyId,
                TypeId = request.TypeId,
            };
            
            await _cognitiveRepository.AddAsync(exercise);
        }

        public async Task UpdateCognitiveExercise(CognitiveExerciseUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found");
            
            var exercise = await _cognitiveRepository.GetByIdAsync(request.Id);
            if (exercise is null)
                throw new ArgumentException("Exercise not found");
            
            if (exercise.UserId != user.Id)
                throw new UnauthorizedAccessException("User cannot change the exercise");
            
            exercise.Title = request.Title;
            exercise.Description = request.Description;
            exercise.DifficultyId = request.DifficultyId;
            exercise.TypeId = request.TypeId;
            
            await _cognitiveRepository.UpdateAsync(exercise);
        }

        public async Task DeleteCognitiveExercise(CognitiveExerciseDeleteRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found");
            
            var exercise = await _cognitiveRepository.GetByIdAsync(request.CognitiveExerciseId);
            if (exercise is null)
                throw new ArgumentException("Exercise not found");
            
            if (exercise.UserId != user.Id)
                throw new UnauthorizedAccessException("User cannot change the exercise");
            
            await _cognitiveRepository.DeleteAsync(exercise);
        }

        public async Task ReportResultAsync(CognitiveExerciseResultRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found");

            var exercise = await _cognitiveRepository.GetByIdAsync(request.ExerciseId);
            if (exercise is null)
                throw new ArgumentException("Exercise not found");

            var result = new CognitiveResult
            {
                UserId = request.UserId,
                ExerciseId = request.ExerciseId,
                Score = request.Score,
                CompletedAt = request.CompletedAt
            };

            await _resultRepository.AddAsync(result);
        }

        public async Task<IEnumerable<CognitiveExerciseResultDto>> GetUserResultsAsync(long userId)
        {
            var results = await _resultRepository.GetByUserIdAsync(userId);
            return results.Select(MapToCognitiveExerciseResultDto);
        }

        private async Task ValidateCreateRequest(CognitiveExerciseCreateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");
            
            var cognitiveTypeExist = await _cognitiveExerciseTypeRepository.ExistItemAsync(x => x.Id == request.TypeId);
            if(cognitiveTypeExist is false)
                throw new ArgumentException("Type not found");
            
            var difficultyExist  = await _difficultyRepository.ExistItemAsync(x => x.Id == request.DifficultyId);
            if(difficultyExist is false)
                throw new ArgumentException("Difficulty not found");
        }
        
        private CognitiveExerciseDto MapToCognitiveExerciseDto(CognitiveExercise entity)
        {
            return new CognitiveExerciseDto()
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Difficulty = entity.Difficulty.Name,
                Type = entity.Type.Name,
                UserId = entity.UserId
            };
        }

        private CognitiveExerciseResultDto MapToCognitiveExerciseResultDto(CognitiveResult entity)
        {
            return new CognitiveExerciseResultDto()
            {
                Id = entity.Id,
                Score = entity.Score,
                CompletedAt = entity.CompletedAt,
                ExerciseId = entity.ExerciseId,
                UserId = entity.UserId
            };
        }
    }
}