using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Interfaces.Services;
using Contatos.Domain.Services;
using Contatos.Domain.Validators;
using Contatos.Infra.Data.Contexts;
using Contatos.Infra.Data.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Contatos.Api.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Método para adicionar as dependências de serviços de domínio.
    /// </summary>
    public static IServiceCollection AddDomainService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssemblyContaining<UsuarioRequestValidator>();

        return services;
    }

    /// <summary>
    /// Método para adicionar as dependências da infraestrutura, como repositórios, contextos, etc.
    /// </summary>        
    public static IServiceCollection AddInfraStructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Método para configurar autenticação JWT.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
                };
            });

        return services;
    }
}