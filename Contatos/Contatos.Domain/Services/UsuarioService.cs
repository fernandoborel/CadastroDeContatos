using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Entities;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Interfaces.Services;
using Contatos.Domain.ValueObjects;
using FluentValidation;

namespace Contatos.Domain.Services;

public class UsuarioService(
    IUnitOfWork uow,
    IValidator<UsuarioRequest> criarValidator,
    IValidator<AtualizarUsuarioRequest> atualizarValidator) : IUsuarioService
{
    public async Task<UsuarioResponse> AtualizarAsync(AtualizarUsuarioRequest request)
    {
        await atualizarValidator.ValidateAndThrowAsync(request);

        var usuario = await uow.UsuarioRepository.FirstOrDefaultAsync(c => c.Id == request.Id);
        if (usuario == null)
            throw new ApplicationException("Usuário não encontrado.");

        usuario.Nome = request.Nome;

        await uow.UsuarioRepository.UpdateAsync(usuario);
        await uow.SaveChangesAsync();

        return ToResponse(usuario);
    }

    public async Task<UsuarioResponse> CriarAsync(UsuarioRequest request)
    {
        await criarValidator.ValidateAndThrowAsync(request);

        //Capturar o email do usuário (ValueObject já executa a validação)
        var email = new Email(request.Email);

        //Regra: Email único no banco de dados
        var existente = await uow.UsuarioRepository.CountAsync(c => c.Email.Endereco.Equals(email.Endereco));
        if (existente > 0)
            throw new ApplicationException("O email informado já está cadastrado. Tente outro.");

        //Criando o usuário
        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha)
        };

        //Salvando no banco de dados
        await uow.UsuarioRepository.AddAsync(usuario);
        await uow.SaveChangesAsync();

        //retornar os dados
        return ToResponse(usuario);
    }

    public async Task<UsuarioResponse> AtualizarStatusAsync(AtualizarStatusUsuarioRequest request)
    {
        var usuario = await uow.UsuarioRepository.FirstOrDefaultAsync(c => c.Id == request.Id);
        if (usuario == null)
            throw new ApplicationException("Usuário não encontrado.");

        usuario.Ativo = request.Ativo;

        await uow.UsuarioRepository.UpdateAsync(usuario);
        await uow.SaveChangesAsync();

        return ToResponse(usuario);
    }

    public async Task<UsuarioResponse> ObterAsync(string email)
    {
        var usuario = await uow.UsuarioRepository.FirstOrDefaultAsync(c => c.Email.Endereco.Equals(email));

        if (usuario == null)
            throw new ApplicationException("Usuário não encontrado.");

        return ToResponse(usuario);
    }

    public async Task<PagedResponse<UsuarioResponse>> ObterTodosAsync(int pagina, int tamanhoPagina)
    {
        if (pagina < 1)
            throw new ArgumentException("A página deve ser maior que zero.");

        if (tamanhoPagina < 1)
            throw new ArgumentException("O tamanho da página deve ser maior que zero.");

        var (data, total) = await uow.UsuarioRepository.GetPageAsync(pagina, tamanhoPagina);

        var totalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina);

        return new PagedResponse<UsuarioResponse>(
            data.Select(ToResponse),
            total,
            pagina,
            tamanhoPagina,
            totalPaginas
        );
    }

    private UsuarioResponse ToResponse(Usuario usuario)
    {
        return new UsuarioResponse(
                usuario.Id,
                usuario.Nome,
                usuario.Email?.Endereco ?? string.Empty,
                usuario.Ativo,
                usuario.DataCadastro
            );
    }
}