using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class Grupo
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public TipoGrupo Tipo { get; private set; }
    public string? Bairro { get; private set; }
    public string Cidade { get; private set; } = default!;
    public string Estado { get; private set; } = default!;
    public string? EnderecoCompleto { get; private set; }
    public long CriadoPorUsuarioId { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public bool Ativo { get; private set; } = true;

    protected Grupo() { }

    public static Grupo CriarBairro(string nome, string bairro, string cidade, string estado, long criadoPorUsuarioId)
    {
        return new Grupo
        {
            Nome = nome,
            Tipo = TipoGrupo.Bairro,
            Bairro = bairro,
            Cidade = cidade,
            Estado = estado,
            CriadoPorUsuarioId = criadoPorUsuarioId
        };
    }

    public static Grupo CriarCondominio(string nome, string enderecoCompleto, string cidade, string estado, long criadoPorUsuarioId)
    {
        return new Grupo
        {
            Nome = nome,
            Tipo = TipoGrupo.Condominio,
            EnderecoCompleto = enderecoCompleto,
            Cidade = cidade,
            Estado = estado,
            CriadoPorUsuarioId = criadoPorUsuarioId
        };
    }
}