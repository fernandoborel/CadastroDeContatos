using Contatos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contatos.Infra.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Endereco)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(100);

            email.HasIndex(e => e.Endereco)
                .IsUnique();
        });

        builder.Property(u => u.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.DataCadastro)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");
    }
}