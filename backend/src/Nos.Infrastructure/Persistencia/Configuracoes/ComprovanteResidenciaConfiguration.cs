using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class ComprovanteResidenciaConfiguration : IEntityTypeConfiguration<ComprovanteResidencia>
{
    public void Configure(EntityTypeBuilder<ComprovanteResidencia> builder)
    {
        builder.ToTable("ComprovanteResidencia");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ArquivoUrl).HasMaxLength(500).IsRequired();
        builder.Property(c => c.MotivoRejeicao).HasMaxLength(500);

        builder.HasIndex(c => c.ParticipacaoGrupoId);
        builder.HasIndex(c => c.Status);
    }
}