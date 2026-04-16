using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;

namespace Contatos.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
}
