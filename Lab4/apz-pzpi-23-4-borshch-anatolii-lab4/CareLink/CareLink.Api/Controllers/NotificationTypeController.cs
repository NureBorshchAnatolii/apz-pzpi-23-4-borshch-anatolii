using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/notiffication-types")]
    [Authorize]
    public class NotificationTypeController : ControllerBase
    {
        private readonly INotificationTypeRepository _notifficationTypeRepository;

        public NotificationTypeController(INotificationTypeRepository notifficationTypeRepository)
        {
            _notifficationTypeRepository = notifficationTypeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var roles = await _notifficationTypeRepository.GetAllAsync();
            return Ok(ApiResponse<IReadOnlyCollection<NotificationType>>.Ok(roles));
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> CreateNotificationType([FromBody] string deviceType)
        {
            var existedType = await _notifficationTypeRepository.ExistItemAsync(x => x.Name.ToLower() == deviceType.Trim().ToLower());
            if (existedType)
                return BadRequest($"Device type {deviceType} already exists");
            
            await _notifficationTypeRepository.AddAsync(new NotificationType() { Name = deviceType });
            return Ok(ApiResponse.Ok("Created"));
        }

        [HttpPut("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateNotificationType(long typeId, [FromBody] string deviceType)
        {
            var existedType = await _notifficationTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {deviceType} not found");
            
            existedType.Name = deviceType;
            await _notifficationTypeRepository.UpdateAsync(existedType);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{typeId:long}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteNotificationType(long typeId)
        {
            var existedType = await _notifficationTypeRepository.GetByIdAsync(typeId);
            if (existedType == null)
                return BadRequest($"Device type {typeId} not found");
            
            await _notifficationTypeRepository.DeleteAsync(existedType);
            return Ok(ApiResponse.Ok());
        }
    }
}