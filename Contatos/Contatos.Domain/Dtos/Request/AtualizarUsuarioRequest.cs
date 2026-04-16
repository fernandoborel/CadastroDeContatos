namespace Contatos.Domain.Dtos.Request;

public record AtualizarUsuarioRequest(
    Guid Id,
    string Nome
);

public record AtualizarUsuarioBodyRequest(
    string Nome
);