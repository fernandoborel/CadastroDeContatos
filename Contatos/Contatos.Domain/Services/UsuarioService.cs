using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Entities;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Interfaces.Services;
using Contatos.Domain.ValueObjects;

namespace Contatos.Domain.Services;

public class UsuarioService(IUnitOfWork uow) : IUsuarioService
{
    public async Task<UsuarioResponse> CriarAsync(UsuarioRequest request)
    {
        //Verificar o nome do cliente
        if (string.IsNullOrEmpty(request.Nome) || request.Nome.Trim().Length < 6)
            throw new ArgumentException("Nome do cliente é obrigatório e deve ter pelo menos 6 caracteres.");

        //Capturar o email do cliente (ValueObject já executa a validação)
        var email = new Email(request.Email);

        //Regra: Email único no banco de dados
        var existente = await uow.UsuarioRepository.CountAsync(c => c.Email.Endereco.Equals(email.Endereco));
        if (existente > 0)
            throw new ApplicationException("O email informado já está cadastrado. Tente outro.");

        //Criando o cliente
        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = email
        };

        //Salvando no banco de dados
        await uow.UsuarioRepository.AddAsync(usuario);
        await uow.SaveChangesAsync();

        //retornar os dados
        return ToResponse(usuario);
    }

    public async Task<UsuarioResponse> ObterAsync(string email)
    {
        var usuario = await uow.UsuarioRepository.FirstOrDefaultAsync(c => c.Email.Endereco.Equals(email));

        if (usuario == null)
            throw new ApplicationException("Usuário não encontrado.");

        return ToResponse(usuario);
    }

    private UsuarioResponse ToResponse(Usuario usuario)
    {
        return new UsuarioResponse(
                usuario.Id,
                usuario.Nome,
                usuario.Email.Endereco,
                usuario.Ativo,
                usuario.DataCadastro
            );
    }
}