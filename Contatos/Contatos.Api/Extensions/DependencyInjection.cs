using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Interfaces.Services;
using Contatos.Domain.Services;
using Contatos.Infra.Data.Contexts;
using Contatos.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Contatos.Api.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Método para adicionar as dependências de serviços de domínio.
    /// </summary>
    public static IServiceCollection AddDomainService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        return services;
    }


    /// <summary>
    /// Método para adicionar as dependências da infraestrutura, como repositórios, contextos, etc.
    /// </summary>        
    public static IServiceCollection AddInfraStructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Configurar o DbContext do Entity Framework
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                //Obter a string de conexão do arquivo de configuração (appsettings.json)
                configuration.GetConnectionString("DefaultConnection")));

        //uow
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}