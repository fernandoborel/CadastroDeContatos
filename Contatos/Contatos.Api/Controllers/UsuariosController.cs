using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contatos.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    [ProducesResponseType(typeof(UsuarioResponse), 201)]
    [HttpPost("criar")]
    public async Task<IActionResult> Criar([FromBody] UsuarioRequest request)
    {
        var usuario = await service.CriarAsync(request);
        return StatusCode(201, usuario);
    }

    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [HttpGet("obter/{email}")]
    public async Task<IActionResult> ObterAsync(string email)
    {
        var usuario = await service.ObterAsync(email);
        return StatusCode(200, usuario);
    }
}