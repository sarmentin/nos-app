using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class ParticipacaoGrupoConfiguration : IEntityTypeConfiguration<ParticipacaoGrupo>
{
    public void Configure(EntityTypeBuilder<ParticipacaoGrupo> builder)
    {
        builder.ToTable("ParticipacaoGrupo");
        builder.HasKey(p => p.Id);

        // 1 usuario so pode ter 1 participacao por grupo
        builder.HasIndex(p => new { p.UsuarioId, p.GrupoId }).IsUnique();

        builder.HasIndex(p => new { p.GrupoId, p.Status });

        // Usado pelo job que verifica prazos de 24h expirados
        builder.HasIndex(p => p.PrazoComprovante)
            .HasFilter("[Status] = 1"); // 1 = AguardandoComprovante

        builder.Property(p => p.MotivoRemocao).HasMaxLength(500);
    }
}