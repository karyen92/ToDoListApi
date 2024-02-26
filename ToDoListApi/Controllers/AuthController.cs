using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ToDoListApi.Handlers.Login;
using ToDoListApi.Handlers.Registration;
using ToDoListApi.Handlers.Users;

namespace ToDoListApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation("Login to System Using Username and Password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody]LoginRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation("Register to The System")]
    public async Task<IActionResult> Register([FromBody]RegisterUserRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("currentUser")]
    [SwaggerOperation("Get Current Sign In User information")]
    public async Task<IActionResult> CurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserRequest());
        return Ok(result);
    }
}