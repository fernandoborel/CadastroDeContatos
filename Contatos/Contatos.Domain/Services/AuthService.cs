using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Contatos.Domain.Services;

public class AuthService(
    IUnitOfWork uow,
    IConfiguration config,
    IValidator<LoginRequest> validator) : IAuthService
{
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        await validator.ValidateAndThrowAsync(request);

        var usuario = await uow.UsuarioRepository.FirstOrDefaultAsync(u => u.Email!.Endereco == request.Email.ToLower());
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
            throw new ApplicationException("Email ou senha inválidos.");

        if (!usuario.Ativo)
            throw new ApplicationException("Usuário inativo.");

        return GerarToken(usuario);
    }

    private TokenResponse GerarToken(Entities.Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email!.Endereco!)
        };

        var expiracao = DateTime.UtcNow.AddHours(int.Parse(config["Jwt:ExpiracaoHoras"]!));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiracao,
            signingCredentials: creds
        );

        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
