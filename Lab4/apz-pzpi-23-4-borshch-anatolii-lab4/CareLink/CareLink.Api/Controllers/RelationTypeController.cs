using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/relation-types")]
    [Authorize]
    public class RelationTypeController : ControllerBase
    {
        private readonly IRelationTypeRepository _relationTypeRepository;

        public RelationTypeController(IRelationTypeRepository relationTypeRepository)
        {
            _relationTypeRepository = relationTypeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRelationTypes()
        {
            var types = await _relationTypeRepository.GetAllAsync();
            return Ok(ApiResponse<IReadOnlyCollection<RelationType>>.Ok(types));
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> CreateRelationType([FromBody] string deviceType)
        {
            var existedType = await _relationTypeRepository.ExistItemAsync(x => x.Name.ToLower() == deviceType.Trim().ToLower());
            if (existedType)
                return BadRequest($"Device type {deviceType} already exists");
            
            await _relationTypeRepository.AddAsync(new RelationType() { Name = deviceType });
            return Ok(ApiResponse.Ok("Created"));
        }

        [HttpPut("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateRelationType(long typeId, [FromBody] string deviceType)
        {
            var existedType = await _relationTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {deviceType} not found");
            
            existedType.Name = deviceType;
            await _relationTypeRepository.UpdateAsync(existedType);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteRelationType(long typeId)
        {
            var existedType = await _relationTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {typeId} not found");
            
            await _relationTypeRepository.DeleteAsync(existedType);
            return Ok(ApiResponse.Ok());
        }
    }
}