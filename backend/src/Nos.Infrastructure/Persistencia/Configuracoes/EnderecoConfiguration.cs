using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Endereco");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rotulo).HasMaxLength(50);
        builder.Property(e => e.Cep).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Rua).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Numero).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Complemento).HasMaxLength(100);
        builder.Property(e => e.Bairro).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Cidade).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Estado).HasMaxLength(2).IsRequired();

        builder.HasIndex(e => e.UsuarioId);
    }
}