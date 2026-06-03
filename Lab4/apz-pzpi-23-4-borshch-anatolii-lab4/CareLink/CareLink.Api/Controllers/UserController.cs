using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserContext _userContext;
        private readonly IUserProfileService _userProfileService;

        public UserController(IUserProfileService userProfileService, IUserContext userContext)
        {
            _userProfileService = userProfileService;
            _userContext = userContext;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetUserProfileDataAsync(long id)
        {
            var userDto = await _userProfileService.GetProfileAsync(id);
            return Ok(ApiResponse<UserProfileDto?>.Ok(userDto));
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserProfileDataAsync()
        {
            var id = _userContext.GetApplicationUserId();
            
            var userDto = await _userProfileService.GetProfileAsync(id);
            return Ok(ApiResponse<UserProfileDto?>.Ok(userDto));
        }
        
        [HttpPut]
        public async Task<IActionResult> UpdateUserProfileDataAsync(UpdateUserProfileRequest request)
        {
            var id = _userContext.GetApplicationUserId();
            
            await _userProfileService.UpdateProfileAsync(id, request);
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserProfileDataAsync()
        {
            var id = _userContext.GetApplicationUserId();
            
            await _userProfileService.DeleteProfileAsync(id);
            return Ok(ApiResponse.Ok());
        }
    }
}