using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentario");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Conteudo).IsRequired();

        builder.HasIndex(c => c.PostId);
    }
}