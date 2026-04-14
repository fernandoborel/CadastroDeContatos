using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;

namespace Contatos.Domain.Interfaces.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse> CriarAsync(UsuarioRequest request);
    Task<UsuarioResponse> ObterAsync(string email);
}