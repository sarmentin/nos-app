using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class ConviteLinkConfiguration : IEntityTypeConfiguration<ConviteLink>
{
    public void Configure(EntityTypeBuilder<ConviteLink> builder)
    {
        builder.ToTable("ConviteLink");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Token).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Token).IsUnique();

        // Garante "1 link ativo por grupo" diretamente no banco
        builder.HasIndex(c => c.GrupoId)
            .IsUnique()
            .HasFilter("[Ativo] = 1");
    }
}