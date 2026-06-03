using CareLink.Api.Models.Requests;
using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos;
using CareLink.Application.Dtos.IoTDevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/iot-devices")]
    [Authorize]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IIoTDeviceService _iotDeviceService;
        private readonly IUserContext _userContext;


        public IoTDeviceController(IIoTDeviceService iotDeviceService, IUserContext userContext)
        {
            _iotDeviceService = iotDeviceService;
            _userContext = userContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllUsersIotDevicesAsync()
        {
            var userId = _userContext.GetApplicationUserId();
            
            var devices = await _iotDeviceService.GetAllUserDevicesAsync(userId);

            return Ok(ApiResponse<IEnumerable<IoTDeviceDto>>.Ok(devices));
        }

        [HttpGet("{serialNumber}/state")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIotDeviceStateAsync(string serialNumber)
        {
            var state = await _iotDeviceService.GetDeviceStateBySerialNumberIdAsync(serialNumber);
            
            return Ok(ApiResponse<IoTDeviceStateDto>.Ok(state));
        }

        [HttpPut("{serialNumber}/state")]
        public async Task<IActionResult> ChangeDeviceState(string serialNumber)
        {
            var userId = _userContext.GetApplicationUserId();

            await _iotDeviceService.ChangeDeviceState(serialNumber);

            return Ok(ApiResponse.Ok("Changed"));
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateIotDeviceAsync(CreateIoTDeviceRequest request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new IoTDeviceCreateRequest(userId, request.SerialNumber, request.DeviceTypeId);
            await _iotDeviceService.CreateDeviceAsync(command);
            
            return Ok(ApiResponse.Ok());
        }

        [HttpPut("{deviceId:long}")]
        public async Task<IActionResult> UpdateIotDeviceAsync(long deviceId, UpdateIoTDeviceRequest request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new IoTDeviceCreateRequest(userId, request.SerialNumber, request.DeviceTypeId);
            await _iotDeviceService.CreateDeviceAsync(command);
            
            return Ok(ApiResponse.Ok());
        }
    }
}