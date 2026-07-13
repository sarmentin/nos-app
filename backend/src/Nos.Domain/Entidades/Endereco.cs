namespace Nos.Domain.Entidades;

public class Endereco
{
    public long Id { get; private set; }
    public long UsuarioId { get; private set; }
    public string? Rotulo { get; private set; }
    public string Cep { get; private set; } = default!;
    public string Rua { get; private set; } = default!;
    public string Numero { get; private set; } = default!;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = default!;
    public string Cidade { get; private set; } = default!;
    public string Estado { get; private set; } = default!;
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public bool Ativo { get; private set; } = true;

    protected Endereco() { }

    public static Endereco Criar(long usuarioId, string cep, string rua, string numero,
        string? complemento, string bairro, string cidade, string estado, string? rotulo)
    {
        return new Endereco
        {
            UsuarioId = usuarioId,
            Cep = cep,
            Rua = rua,
            Numero = numero,
            Complemento = complemento,
            Bairro = bairro,
            Cidade = cidade,
            Estado = estado,
            Rotulo = rotulo
        };
    }
}