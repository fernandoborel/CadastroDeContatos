using Contatos.Domain.Interfaces.Repositories;
using Contatos.Infra.Data.Contexts;

namespace Contatos.Infra.Data.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public IUsuarioRepository UsuarioRepository => new UsuarioRepository(context);

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    public void Dispose()
        => context.Dispose();
}