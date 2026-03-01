using InTagLogicLayer.Interfaces;
using InTagViewModelLayer.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers.Api
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseVm>> Login([FromBody] LoginRequestVm request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new { data = response });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { errors = new[] { "Invalid email or password." } });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseVm>> Register([FromBody] RegisterRequestVm request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(new { data = response });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseVm>> Refresh([FromBody] RefreshTokenRequestVm request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(new { data = response });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { errors = new[] { "Invalid or expired refresh token." } });
            }
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(refreshToken);
                return Ok(new { data = "Token revoked." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }
    }
}
