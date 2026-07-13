namespace Nos.Domain.Enums;

public enum TipoGrupo { Bairro = 1, Condominio = 2 }

public enum PapelParticipacao { Membro = 1, Admin = 2 }

public enum OrigemEntrada { Link = 1, Busca = 2 }

public enum StatusParticipacao
{
    AguardandoAprovacaoSolicitacao = 0,
    AguardandoComprovante = 1,
    AguardandoValidacaoAdmin = 2,
    Verificado = 3,
    Removido = 4
}

public enum StatusComprovante { Pendente = 0, Aprovado = 1, Rejeitado = 2 }

public enum CategoriaPost
{
    Diario = 1,
    CompraVenda = 2,
    Alertas = 3,
    AchadosPerdidos = 4,
    Eventos = 5,
    Recomendacoes = 6,
    Reclamacoes = 7
}

public enum NivelUrgencia { Baixo = 1, Medio = 2, Alto = 3 }

public enum StatusPost { Ativo = 1, OcultoAutomatico = 2, RemovidoModeracao = 3 }

public enum MotivoDenuncia
{
    Spam = 1,
    ConteudoImproprio = 2,
    DiscriminacaoOuPerfilamento = 3,
    Outro = 4
}