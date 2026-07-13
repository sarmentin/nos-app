using Nos.Domain.Enums;

namespace Nos.Domain.Entidades;

public class ComprovanteResidencia
{
    public long Id { get; private set; }
    public long ParticipacaoGrupoId { get; private set; }
    public string ArquivoUrl { get; private set; } = default!;
    public StatusComprovante Status { get; private set; } = StatusComprovante.Pendente;
    public string? MotivoRejeicao { get; private set; }
    public long? RevisadoPorUsuarioId { get; private set; }
    public DateTime DataEnvio { get; private set; } = DateTime.UtcNow;
    public DateTime? DataRevisao { get; private set; }

    protected ComprovanteResidencia() { }

    public static ComprovanteResidencia Enviar(long participacaoGrupoId, string arquivoUrl)
    {
        return new ComprovanteResidencia
        {
            ParticipacaoGrupoId = participacaoGrupoId,
            ArquivoUrl = arquivoUrl
        };
    }

    public void Aprovar(long revisadoPorUsuarioId)
    {
        Status = StatusComprovante.Aprovado;
        RevisadoPorUsuarioId = revisadoPorUsuarioId;
        DataRevisao = DateTime.UtcNow;
    }

    public void Rejeitar(long revisadoPorUsuarioId, string motivo)
    {
        Status = StatusComprovante.Rejeitado;
        MotivoRejeicao = motivo;
        RevisadoPorUsuarioId = revisadoPorUsuarioId;
        DataRevisao = DateTime.UtcNow;
    }
}