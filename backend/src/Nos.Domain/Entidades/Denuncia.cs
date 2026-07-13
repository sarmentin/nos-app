using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class Denuncia
{
    public long Id { get; private set; }
    public long PostId { get; private set; }
    public long UsuarioId { get; private set; }
    public MotivoDenuncia Motivo { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    protected Denuncia() { }

    public static Denuncia Criar(long postId, long usuarioId, MotivoDenuncia motivo)
        => new() { PostId = postId, UsuarioId = usuarioId, Motivo = motivo };
}