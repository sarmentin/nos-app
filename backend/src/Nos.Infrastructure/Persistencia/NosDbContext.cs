using Microsoft.EntityFrameworkCore;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Persistencia;

public class NosDbContext : DbContext
{
    public NosDbContext(DbContextOptions<NosDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<ConviteLink> ConvitesLink => Set<ConviteLink>();
    public DbSet<ParticipacaoGrupo> ParticipacoesGrupo => Set<ParticipacaoGrupo>();
    public DbSet<ComprovanteResidencia> ComprovantesResidencia => Set<ComprovanteResidencia>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Denuncia> Denuncias => Set<Denuncia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}