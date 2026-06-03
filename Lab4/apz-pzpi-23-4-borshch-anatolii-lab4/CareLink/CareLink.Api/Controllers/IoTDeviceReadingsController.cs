using CareLink.Api.Models.Requests;
using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.IoTDevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/iot-readings")]
    [Authorize]
    public class IoTDeviceReadingsController : ControllerBase
    {
        private readonly IIoTReadingService _readingService;
        private readonly IUserContext _userContext;

        public IoTDeviceReadingsController(
            IIoTReadingService readingService,
            IUserContext userContext)
        {
            _readingService = readingService;
            _userContext = userContext;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateReadingAsync([FromBody] CreateIoTReadingRequest request)
        {
            var command = new IoTReadingCreateRequest(
                request.ReadDateTime,
                request.Pulse,
                request.ActivityLevel,
                request.Temperature,
                request.SerialNumber);

            var result = await _readingService.CreateReadingAsync(command);

            return Ok(result);
        }

        [HttpGet("{userId:long}")]
        public async Task<IActionResult> GetAllReadingsByUserAsync(long userId)
        {
            var result = await _readingService.GetAllReadingsByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<IoTReadingDto>>.Ok(result));
        }

        [HttpGet("{userId:long}/latest")]
        public async Task<IActionResult> GetLatestReadingsByUserAsync(long userId,[FromQuery] int count)
        {
            var result = await _readingService.GetLatestReadingsByUserAsync(userId, count);
            return Ok(ApiResponse<IEnumerable<IoTReadingDto>>.Ok(result));
        }

        [HttpGet("{userId:long}/range")]
        public async Task<IActionResult> GetReadingsInRangeByUserAsync(
            long userId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _readingService.GetReadingsInRangeByUserAsync(userId, from, to);
            return Ok(ApiResponse<IEnumerable<IoTReadingDto>>.Ok(result));
        }
    }
}