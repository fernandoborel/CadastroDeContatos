using Contatos.Infra.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Contatos.Infra.Data.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    //mapeamento
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
    }
}