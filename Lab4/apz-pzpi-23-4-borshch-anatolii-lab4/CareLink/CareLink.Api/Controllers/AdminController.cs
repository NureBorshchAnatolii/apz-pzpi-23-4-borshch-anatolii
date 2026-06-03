using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Admin;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        
        [HttpGet("system-state")]
        public async Task<IActionResult> GetSystemState()
        {
            var state = await _adminService.GetSystemStateAsync();
            return Ok(state);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<AdminUserDto>>.Ok(users));
        }
        
        [HttpPut("users/{userId:long}/role/{roleId:long}")]
        public async Task<IActionResult> ChangeUserRole(long userId, long roleId)
        {
            await _adminService.ChangeUserRoleAsync(userId, roleId);
            return NoContent();
        }
        
        [HttpDelete("users/{userId:long}")]
        public async Task<IActionResult> DeleteUser(long userId)
        {
            await _adminService.DeleteUserAsync(userId);
            return NoContent();
        }
        
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? level)
        {
            var logs = await _adminService.GetLogsAsync(from, to, level);
            return Ok(ApiResponse<IEnumerable<SystemLogDto>>.Ok(logs));
        }
    }
}