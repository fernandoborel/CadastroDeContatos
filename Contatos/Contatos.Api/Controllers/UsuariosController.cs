using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contatos.Api.Controllers;

[Authorize]
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

    [ProducesResponseType(typeof(PagedResponse<UsuarioResponse>), 200)]
    [HttpGet("listar")]
    public async Task<IActionResult> ListarAsync([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 10)
    {
        var resultado = await service.ObterTodosAsync(pagina, tamanhoPagina);
        return StatusCode(200, resultado);
    }

    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [HttpGet("obter")]
    public async Task<IActionResult> ObterAsync([FromQuery] string email)
    {
        var usuario = await service.ObterAsync(email);
        return StatusCode(200, usuario);
    }

    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [HttpPut("atualizar/{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioBodyRequest body)
    {
        var request = new AtualizarUsuarioRequest(id, body.Nome);
        var usuario = await service.AtualizarAsync(request);
        return StatusCode(200, usuario);
    }

    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [HttpPut("atualizar-status/{id}")]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusUsuarioBodyRequest body)
    {
        var request = new AtualizarStatusUsuarioRequest(id, body.Ativo);
        var usuario = await service.AtualizarStatusAsync(request);
        return StatusCode(200, usuario);
    }
}