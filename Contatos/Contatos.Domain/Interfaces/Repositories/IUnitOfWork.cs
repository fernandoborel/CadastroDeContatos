namespace Contatos.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync();
    public IUsuarioRepository UsuarioRepository { get; }
}