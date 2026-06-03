using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/difficulties")]
    [Authorize]
    public class DifficultyController : ControllerBase
    {
        private readonly IDifficultyRepository _difficultyRepository;

        public DifficultyController(IDifficultyRepository difficultyRepository)
        {
            _difficultyRepository = difficultyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDifficultiesAsync()
        {
            var types = await _difficultyRepository.GetAllAsync();
            return Ok(ApiResponse<IReadOnlyCollection<Difficulty>>.Ok(types));
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> CreateDifficulty([FromBody] string deviceType)
        {
            var existedType = await _difficultyRepository.ExistItemAsync(x => x.Name.ToLower() == deviceType.Trim().ToLower());
            if (existedType)
                return BadRequest($"Device type {deviceType} already exists");
            
            await _difficultyRepository.AddAsync(new Difficulty() { Name = deviceType });
            return Ok(ApiResponse.Ok());
        }

        [HttpPut("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateDifficulty(long typeId, [FromBody] string deviceType)
        {
            var existedType = await _difficultyRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {deviceType} not found");
            
            existedType.Name = deviceType;
            await _difficultyRepository.UpdateAsync(existedType);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteDifficulty(long typeId)
        {
            var existedType = await _difficultyRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {typeId} not found");
            
            await _difficultyRepository.DeleteAsync(existedType);
            return Ok(ApiResponse.Ok());
        }
    }
}