using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using Shared.Domain.Types;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;

namespace WAPIIdentity.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")]
    [Authorize(Roles = nameof(RoleType.Admin))]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterUserRequest model, 
        CancellationToken cancellationToken)
    {
        RegisterUserResponse? result = await _userService.RegisterAsync(model, cancellationToken);
        return Ok(result);
    }
}