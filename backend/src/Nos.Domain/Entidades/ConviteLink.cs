namespace Nos.Domain.Entidades;

public class ConviteLink
{
    public long Id { get; private set; }
    public long GrupoId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime? DataExpiracao { get; private set; }
    public int? LimiteUsos { get; private set; }
    public int UsosAtuais { get; private set; }
    public bool Ativo { get; private set; } = true;

    protected ConviteLink() { }

    public static ConviteLink Gerar(long grupoId, DateTime? dataExpiracao, int? limiteUsos)
    {
        return new ConviteLink
        {
            GrupoId = grupoId,
            Token = Guid.NewGuid().ToString("N"),
            DataExpiracao = dataExpiracao,
            LimiteUsos = limiteUsos
        };
    }

    public bool EstaValido()
    {
        if (!Ativo) return false;
        if (DataExpiracao is not null && DataExpiracao < DateTime.UtcNow) return false;
        if (LimiteUsos is not null && UsosAtuais >= LimiteUsos) return false;
        return true;
    }

    public void RegistrarUso()
    {
        if (!EstaValido())
            throw new InvalidOperationException("Este convite não é mais válido.");
        UsosAtuais++;
    }

    public void Revogar() => Ativo = false;
}