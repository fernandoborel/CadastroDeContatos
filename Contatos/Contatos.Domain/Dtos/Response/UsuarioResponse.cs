namespace Contatos.Domain.Dtos.Response;

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    bool Ativo,
    DateTime DataCadastro
);