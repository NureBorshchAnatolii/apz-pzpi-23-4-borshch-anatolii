using CareLink.Api.Models.Requests;
using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.CognitiveExercise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/cognitive-exercise")]
    [Authorize]
    public class CognitiveExerciseController : ControllerBase
    {
        private readonly ICognitiveExerciseService _cognitiveExerciseService;
        private readonly IUserContext _userContext;


        public CognitiveExerciseController(IUserContext userContext, ICognitiveExerciseService cognitiveExerciseService)
        {
            _userContext = userContext;
            _cognitiveExerciseService = cognitiveExerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCognitiveExercisesAsync()
        {
            var exercises = await _cognitiveExerciseService.GetAllCognitiveExercisesAsync();
            return Ok(ApiResponse<IEnumerable<CognitiveExerciseDto>>.Ok(exercises));
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateCognitiveExercise([FromBody] CreateCognitiveExercise request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new CognitiveExerciseCreateRequest(request.Title, request.Description
                , request.DifficultyId, request.TypeId, userId);
            await _cognitiveExerciseService.CreateCognitiveExercise(command);

            return Ok(ApiResponse.Ok("Exercise was created"));
        }

        [HttpPut("{exerciseId:long}")]
        public async Task<IActionResult> UpdateCognitiveExercise(long exerciseId, [FromBody] UpdateCognitiveExerciseRequest request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new CognitiveExerciseUpdateRequest(exerciseId, request.Title
                , request.Description, request.DifficultyId, request.TypeId,userId);
            await _cognitiveExerciseService.UpdateCognitiveExercise(command);
            
            return Ok(ApiResponse.Ok("Exercise was updated"));
        }

        [HttpDelete("{exerciseId:long}")]
        public async Task<IActionResult> DeleteCognitiveExercise(long exerciseId)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new CognitiveExerciseDeleteRequest(userId, exerciseId);
            await _cognitiveExerciseService.DeleteCognitiveExercise(command);
            
            return Ok(ApiResponse.Ok("Exercise was deleted"));
        }

        [HttpPost("{exerciseId:long}/result")]
        public async Task<IActionResult> ReportCognitiveExerciseResult(long exerciseId, [FromBody] CognitiveExerciseReport request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new CognitiveExerciseResultRequest(exerciseId, request.Score, 
                request.CompletedAt, userId);
            await _cognitiveExerciseService.ReportResultAsync(command);

            return Ok(ApiResponse.Ok("Result was saved"));
        }

        [HttpGet("{userId:long}/result")]
        public async Task<IActionResult> GetUsersCognitiveExerciseResults(long userId)
        {
            var results = await _cognitiveExerciseService.GetUserResultsAsync(userId);
            return Ok(ApiResponse<IEnumerable<CognitiveExerciseResultDto>>.Ok(results));
        }
        
    }
}