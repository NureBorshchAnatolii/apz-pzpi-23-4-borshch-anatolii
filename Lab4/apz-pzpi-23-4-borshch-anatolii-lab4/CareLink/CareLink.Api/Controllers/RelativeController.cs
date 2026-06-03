using CareLink.Api.Models.Requests;
using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Relatives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/relatives")]
    [Authorize]
    public class RelativeController : ControllerBase
    {
        private readonly IUserContext _userContext;
        private readonly IRelativeService _relativeService;
        private readonly IPdfService _pdfService;

        public RelativeController(
            IUserContext userContext, 
            IRelativeService relativeService,
            IPdfService pdfService)
        {
            _userContext = userContext;
            _relativeService = relativeService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRelatives()
        {
            var userId = _userContext.GetApplicationUserId();
            
            var relative = await _relativeService.GetAllRelativesAsync(userId);
            return Ok(ApiResponse<IEnumerable<RelativeDto>>.Ok(relative));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRelativeAsync([FromBody] CreateRelativeRequest request)
        {
            var userId = _userContext.GetApplicationUserId();

            var command = new RelativeCreateCommand(userId, request.RelativeId, request.RelationTypeId);
            await _relativeService.CreateRelativeAsync(command);
            
            return Ok(ApiResponse.Ok());
        }

        [HttpDelete("{relativeId:long}")]
        public async Task<IActionResult> DeleteRelative(long relativeId)
        {
            var userId = _userContext.GetApplicationUserId();
            
            var command = new RelativeDeleteCommand(userId, relativeId);
            await _relativeService.DeleteRelativeAsync(command);
            
            return Ok(ApiResponse.Ok());
        }

        [HttpGet("{relativeId:long}/report")]
        public async Task<IActionResult> GetRelativeReport(long relativeId)
        {
            var guardianUserId = _userContext.GetApplicationUserId();

            byte[] pdfBytes;
            try
            {
                pdfBytes = await _pdfService.GenerateRelativeReportAsync(guardianUserId, relativeId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You are not authorized to view this relative's report.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating report: {ex.Message}");
            }

            return File(pdfBytes, "application/pdf", $"RelativeReport_{relativeId}.pdf");
        }
    }
}