namespace Contatos.Domain.Dtos.Response;

/// <summary>
/// Resposta paginada genérica utilizada em endpoints que retornam listas com paginação.
/// </summary>
/// <typeparam name="T">Tipo do item retornado na listagem.</typeparam>
public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Total,
    int Pagina,
    int TamanhoPagina,
    int TotalPaginas
);
