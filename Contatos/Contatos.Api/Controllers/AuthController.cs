using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contatos.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await service.LoginAsync(request);
        return StatusCode(200, token);
    }
}
