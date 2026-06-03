using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Notiffications;
using CareLink.Security.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;
        
        public NotificationController(INotificationService notificationService, IUserContext userContext)
        {
            _notificationService = notificationService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersNotifications()
        {
            var userId = _userContext.GetApplicationUserId();
            var notifications = await _notificationService.GetUsersNotifications(userId);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications));
        }

        [HttpPut("{notificationId:long}")]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            var userId = _userContext.GetApplicationUserId();
            await _notificationService.MarkAsRead(userId, notificationId);
            return Ok(ApiResponse.Ok());
        }
    }
}