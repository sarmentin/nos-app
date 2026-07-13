using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).HasMaxLength(150).IsRequired();

        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.SenhaHash).HasMaxLength(500).IsRequired();

        builder.Property(u => u.DataCriacao).HasDefaultValueSql("SYSUTCDATETIME()");

        // Navegacao privada _enderecos -> acessa via campo, nao via propriedade publica
        builder.Metadata.FindNavigation(nameof(Usuario.Enderecos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(u => u.Enderecos)
            .WithOne()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}