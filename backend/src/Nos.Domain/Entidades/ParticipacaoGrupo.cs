using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class ParticipacaoGrupo
{
    public long Id { get; private set; }
    public long UsuarioId { get; private set; }
    public long GrupoId { get; private set; }
    public long? EnderecoId { get; private set; }
    public PapelParticipacao Papel { get; private set; } = PapelParticipacao.Membro;
    public OrigemEntrada OrigemEntrada { get; private set; }
    public StatusParticipacao Status { get; private set; }
    public DateTime? PrazoComprovante { get; private set; }
    public string? MotivoRemocao { get; private set; }
    public DateTime DataEntrada { get; private set; } = DateTime.UtcNow;

    public static readonly TimeSpan PrazoParaComprovante = TimeSpan.FromHours(24);

    protected ParticipacaoGrupo() { }

    /// <summary>Entrada via Link: provisória e imediata, sem aprovação prévia.</summary>
    public static ParticipacaoGrupo CriarViaLink(long usuarioId, long grupoId)
    {
        return new ParticipacaoGrupo
        {
            UsuarioId = usuarioId,
            GrupoId = grupoId,
            OrigemEntrada = OrigemEntrada.Link,
            Status = StatusParticipacao.AguardandoComprovante,
            PrazoComprovante = DateTime.UtcNow.Add(PrazoParaComprovante)
        };
    }

    /// <summary>Entrada via Busca: precisa de aprovação do Admin antes de liberar o prazo do comprovante.</summary>
    public static ParticipacaoGrupo CriarViaBusca(long usuarioId, long grupoId)
    {
        return new ParticipacaoGrupo
        {
            UsuarioId = usuarioId,
            GrupoId = grupoId,
            OrigemEntrada = OrigemEntrada.Busca,
            Status = StatusParticipacao.AguardandoAprovacaoSolicitacao
        };
    }

    public void AssociarEndereco(long enderecoId) => EnderecoId = enderecoId;

    public void AprovarSolicitacao()
    {
        if (Status != StatusParticipacao.AguardandoAprovacaoSolicitacao)
            throw new InvalidOperationException("Só é possível aprovar solicitações pendentes.");

        Status = StatusParticipacao.AguardandoComprovante;
        PrazoComprovante = DateTime.UtcNow.Add(PrazoParaComprovante);
    }

    public void RejeitarSolicitacao(string motivo)
    {
        Status = StatusParticipacao.Removido;
        MotivoRemocao = motivo;
    }

    public void RegistrarEnvioComprovante()
    {
        if (Status != StatusParticipacao.AguardandoComprovante)
            throw new InvalidOperationException("Não há prazo de comprovante em aberto para esta participação.");

        // O prazo de 24h deixa de valer a partir daqui - não há mais remoção automática por tempo
        Status = StatusParticipacao.AguardandoValidacaoAdmin;
        PrazoComprovante = null;
    }

    public void AprovarComprovante() => Status = StatusParticipacao.Verificado;

    public void RejeitarComprovante(string motivo)
    {
        Status = StatusParticipacao.Removido;
        MotivoRemocao = motivo;
    }

    /// <summary>Chamado pelo job/rotina que verifica prazos expirados.</summary>
    public bool PrazoExpirado() =>
        Status == StatusParticipacao.AguardandoComprovante
        && PrazoComprovante is not null
        && PrazoComprovante < DateTime.UtcNow;

    public void RemoverPorPrazoExpirado()
    {
        if (!PrazoExpirado())
            throw new InvalidOperationException("O prazo ainda não expirou.");

        Status = StatusParticipacao.Removido;
        MotivoRemocao = "Comprovante de residência não enviado dentro do prazo de 24 horas.";
    }
}