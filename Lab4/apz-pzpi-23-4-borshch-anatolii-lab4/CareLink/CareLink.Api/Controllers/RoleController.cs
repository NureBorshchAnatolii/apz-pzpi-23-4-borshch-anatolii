using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var types = await _roleRepository.GetAllAsync();
            return Ok(ApiResponse<IReadOnlyCollection<Role>>.Ok(types));
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> CreateRole([FromBody] string deviceType)
        {
            var existedType = await _roleRepository.ExistItemAsync(x => x.Name.ToLower() == deviceType.Trim().ToLower());
            if (existedType)
                return BadRequest($"Device type {deviceType} already exists");
            
            await _roleRepository.AddAsync(new Role() { Name = deviceType });
            return Ok(ApiResponse.Ok());
        }

        [HttpPut("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateRole(long typeId, [FromBody] string deviceType)
        {
            var existedType = await _roleRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {deviceType} not found");
            
            existedType.Name = deviceType;
            await _roleRepository.UpdateAsync(existedType);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteRole(long typeId)
        {
            var existedType = await _roleRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {typeId} not found");
            
            await _roleRepository.DeleteAsync(existedType);
            return Ok(ApiResponse.Ok());
        }
    }
}