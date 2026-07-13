using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class DenunciaConfiguration : IEntityTypeConfiguration<Denuncia>
{
    public void Configure(EntityTypeBuilder<Denuncia> builder)
    {
        builder.ToTable("Denuncia");
        builder.HasKey(d => d.Id);

        // 1 denuncia por usuario por post
        builder.HasIndex(d => new { d.PostId, d.UsuarioId }).IsUnique();

        builder.HasIndex(d => new { d.PostId, d.Motivo });
    }
}