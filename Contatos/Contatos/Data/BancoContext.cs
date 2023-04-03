using Contatos.Data.Map;
using Contatos.Models;
using Microsoft.EntityFrameworkCore;

namespace Contatos.Data
{
    public class BancoContext : DbContext
    {
        public BancoContext(DbContextOptions<BancoContext> options) : base(options) 
        {
        }

        public DbSet<ContatoModel> Contatos { get; set;}
        public DbSet<UsuarioModel> Usuarios { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ContatoMap());

            base.OnModelCreating(modelBuilder);
        }
    }
}
