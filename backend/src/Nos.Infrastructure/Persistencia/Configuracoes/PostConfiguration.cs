using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia.Configuracoes;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Post");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Conteudo).IsRequired();

        // Indice para o padrao de acesso mais comum: feed paginado por grupo+categoria
        builder.HasIndex(p => new { p.GrupoId, p.Categoria, p.DataCriacao });

        // Navegacao privada _denuncias -> acessa via campo
        builder.Metadata.FindNavigation(nameof(Post.Denuncias))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Denuncias)
            .WithOne()
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}