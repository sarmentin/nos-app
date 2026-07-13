namespace Nos.Domain.Entidades;

public class Comentario
{
    public long Id { get; private set; }
    public long PostId { get; private set; }
    public long AutorId { get; private set; }
    public string Conteudo { get; private set; } = default!;
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    protected Comentario() { }

    public static Comentario Criar(long postId, long autorId, string conteudo)
        => new() { PostId = postId, AutorId = autorId, Conteudo = conteudo };
}