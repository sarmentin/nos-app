using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class Post
{
    public long Id { get; private set; }
    public long GrupoId { get; private set; }
    public long AutorId { get; private set; }
    public CategoriaPost Categoria { get; private set; }
    public NivelUrgencia? NivelUrgencia { get; private set; }
    public string Titulo { get; private set; } = default!;
    public string Conteudo { get; private set; } = default!;
    public StatusPost Status { get; private set; } = StatusPost.Ativo;
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    private readonly List<Denuncia> _denuncias = new();
    public IReadOnlyCollection<Denuncia> Denuncias => _denuncias.AsReadOnly();

    protected Post() { }

    public const int TamanhoMinimoDescricaoAlerta = 30;
    public const int LimiteDenunciasGerais = 5;
    public const int LimiteDenunciasDiscriminacao = 2;

    public static Post Criar(long grupoId, long autorId, CategoriaPost categoria,
        string titulo, string conteudo, NivelUrgencia? nivelUrgencia)
    {
        if (categoria == CategoriaPost.Alertas)
        {
            if (nivelUrgencia is null)
                throw new ArgumentException("Posts de Alerta exigem nível de urgência.");
            if (conteudo.Length < TamanhoMinimoDescricaoAlerta)
                throw new ArgumentException($"Alertas exigem descrição de pelo menos {TamanhoMinimoDescricaoAlerta} caracteres, descrevendo o fato observado.");
        }

        return new Post
        {
            GrupoId = grupoId,
            AutorId = autorId,
            Categoria = categoria,
            NivelUrgencia = nivelUrgencia,
            Titulo = titulo,
            Conteudo = conteudo
        };
    }

    public void RegistrarDenuncia(long usuarioId, MotivoDenuncia motivo)
    {
        if (_denuncias.Any(d => d.UsuarioId == usuarioId))
            return;

        _denuncias.Add(Denuncia.Criar(Id, usuarioId, motivo));

        if (Status != StatusPost.Ativo) return;

        var totalGeral = _denuncias.Count;
        var totalDiscriminacao = _denuncias.Count(d => d.Motivo == MotivoDenuncia.DiscriminacaoOuPerfilamento);

        if (totalGeral >= LimiteDenunciasGerais || totalDiscriminacao >= LimiteDenunciasDiscriminacao)
            Status = StatusPost.OcultoAutomatico;
    }
}