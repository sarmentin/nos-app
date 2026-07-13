using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class Usuario
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;
    public DateTime AceiteTermosEm { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public bool Ativo { get; private set; } = true;

    private readonly List<Endereco> _enderecos = new();
    public IReadOnlyCollection<Endereco> Enderecos => _enderecos.AsReadOnly();

    protected Usuario() { } // EF Core

    public static Usuario Criar(string nome, string username, string email, string senhaHash, bool aceiteTermos)
    {
        if (!aceiteTermos)
            throw new InvalidOperationException("É necessário aceitar os Termos de Uso para se cadastrar.");

        return new Usuario
        {
            Nome = nome,
            Username = username,
            Email = email,
            SenhaHash = senhaHash,
            AceiteTermosEm = DateTime.UtcNow
        };
    }

    public const int LimiteEnderecos = 3;

    public Endereco AdicionarEndereco(string cep, string rua, string numero, string? complemento,
        string bairro, string cidade, string estado, string? rotulo)
    {
        if (_enderecos.Count(e => e.Ativo) >= LimiteEnderecos)
            throw new InvalidOperationException($"Limite de {LimiteEnderecos} endereços já atingido.");

        var endereco = Endereco.Criar(Id, cep, rua, numero, complemento, bairro, cidade, estado, rotulo);
        _enderecos.Add(endereco);
        return endereco;
    }
}