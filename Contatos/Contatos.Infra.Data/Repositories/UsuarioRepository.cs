using Contatos.Domain.Entities;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Infra.Data.Contexts;

namespace Contatos.Infra.Data.Repositories;

public class UsuarioRepository(AppDbContext context) : BaseRepository<Usuario, Guid>(context), IUsuarioRepository
{}
