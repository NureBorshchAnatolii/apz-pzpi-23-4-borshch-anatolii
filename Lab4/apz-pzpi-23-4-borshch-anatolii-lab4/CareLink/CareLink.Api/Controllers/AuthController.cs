

using CareLink.Api.Models.Requests;
using CareLink.Api.Models.Responses;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos;
using CareLink.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Logs in a user using username/email and password.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>An <see cref="ApiResponse"/> containing JWT and user info.</returns>
        /// <response code="200">Login successful, returns <see cref="ApiResponse{LoginData}"/></response>
        /// <response code="400">Validation failed, returns <see cref="ApiResponse.Errors"/></response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginData>>> Login([FromBody] LoginRequest request)
        {
            var loginDto = new UserLoginDto(request.Password, request.Email);
            
            var loginData = await _authService.Login(loginDto);
            
            return Ok(ApiResponse<LoginData>.Ok(loginData));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <returns>An <see cref="ApiResponse"/> indicating success or failure.</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Validation failed, returns <see cref="ApiResponse.Errors"/></response>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            var registerDto = new UserRegisterDto(request.FirstName, request.LastName, request.RoleId
                , request.Email, request.Password, request.BirthDate, request.Address, request.PhoneNumber);
            
            await _authService.Register(registerDto);

            return Ok(ApiResponse.Ok("User registered successfully"));
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>An <see cref="ApiResponse"/> confirming logout.</returns>
        /// <response code="200">Logout successful</response>
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            await _authService.Logout();
            
            return Ok(ApiResponse.Ok("Endpoint mocked"));
        }
    }
}