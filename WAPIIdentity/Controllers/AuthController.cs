using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;

namespace WAPIIdentity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public AuthController(ILoginService loginService)
        {
            _loginService = loginService;
        }
        
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginRequest model, 
            CancellationToken cancellationToken)
        {
            LoginResponse? result = await _loginService.LoginAsync(model, cancellationToken);
            return Ok(result);
        }
    }
}
