namespace Contatos.Domain.Dtos.Request;

public record AtualizarStatusUsuarioRequest(
    Guid Id,
    bool Ativo
);

public record AtualizarStatusUsuarioBodyRequest(
    bool Ativo
);