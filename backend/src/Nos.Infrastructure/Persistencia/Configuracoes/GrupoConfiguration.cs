using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.ToTable("Grupo");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Nome).HasMaxLength(150).IsRequired();
        builder.Property(g => g.Bairro).HasMaxLength(100);
        builder.Property(g => g.Cidade).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Estado).HasMaxLength(2).IsRequired();
        builder.Property(g => g.EnderecoCompleto).HasMaxLength(300);

        builder.HasIndex(g => new { g.Cidade, g.Bairro });
        builder.HasIndex(g => g.Tipo);
    }
}