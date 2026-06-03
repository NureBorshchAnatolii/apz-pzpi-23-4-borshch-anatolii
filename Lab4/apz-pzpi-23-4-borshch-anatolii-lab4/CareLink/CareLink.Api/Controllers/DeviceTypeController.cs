using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/device-types")]
    [Authorize]
    public class DeviceTypeController : ControllerBase
    {
        private readonly IDeviceTypeRepository _deviceTypeRepository;

        public DeviceTypeController(IDeviceTypeRepository deviceTypeRepository)
        {
            _deviceTypeRepository = deviceTypeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeviceTypesAsync()
        {
            var types = await _deviceTypeRepository.GetAllAsync();
            return Ok(ApiResponse< IReadOnlyCollection<DeviceType>?>.Ok(types));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> CreateDeviceType([FromBody] string deviceType)
        {
            var existedType = await _deviceTypeRepository.ExistItemAsync(x => x.Name.ToLower() == deviceType.Trim().ToLower());
            if (existedType)
                return BadRequest($"Device type {deviceType} already exists");
            
            await _deviceTypeRepository.AddAsync(new DeviceType { Name = deviceType });
            return Ok(ApiResponse.Ok("Created Device Type"));
        }

        [HttpPut("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateDeviceType(long typeId, [FromBody] string deviceType)
        {
            var existedType = await _deviceTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {deviceType} not found");
            
            existedType.Name = deviceType;
            await _deviceTypeRepository.UpdateAsync(existedType);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteDeviceType(long typeId)
        {
            var existedType = await _deviceTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {typeId} not found");
            
            await _deviceTypeRepository.DeleteAsync(existedType);
            return Ok(ApiResponse.Ok());
        }
    }
}