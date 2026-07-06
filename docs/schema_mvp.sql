/* =====================================================================
   N.O.S. - NUCLEO DE ORGANIZACAO SOCIAL
   SCHEMA SQL SERVER - VERSAO ATUALIZADA (pos Fases 0-4)
   =====================================================================
   Principais mudancas em relacao ao rascunho inicial:
   - Removido geocoding/GEOGRAPHY: enderecos sao texto simples, sem
     validacao automatica de area
   - Niveis de verificacao redefinidos:
       Nivel 1 = cadastro basico (Usuario)
       Nivel 2 = ao menos 1 endereco cadastrado, ate 3 (Endereco)
       Nivel 3 = comprovante de residencia (por participacao em grupo,
                 nao e mais um status global do usuario)
   - Entrada em grupo agora tem 2 canais: Link (entrada direta) e
     Busca (exige aprovacao previa do Admin antes do fluxo de
     comprovante)
   - Categoria "Aviso" renomeada para "Diario"
   ===================================================================== */

-- =====================================================================
-- 1. USUARIO E AUTENTICACAO
-- =====================================================================

CREATE TABLE Usuario (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(150) NOT NULL,
    Username        NVARCHAR(50) NOT NULL,
    Email           NVARCHAR(255) NOT NULL,
    SenhaHash       NVARCHAR(500) NOT NULL,
    AceiteTermosEm  DATETIME2 NOT NULL,
    DataCriacao     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Ativo           BIT NOT NULL DEFAULT 1,

    CONSTRAINT UQ_Usuario_Username UNIQUE (Username),
    CONSTRAINT UQ_Usuario_Email UNIQUE (Email)
);

CREATE TABLE RefreshToken (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       BIGINT NOT NULL REFERENCES Usuario(Id),
    Token           NVARCHAR(500) NOT NULL,
    DataExpiracao   DATETIME2 NOT NULL,
    Revogado        BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_RefreshToken_UsuarioId ON RefreshToken(UsuarioId);

CREATE TABLE TokenRedefinicaoSenha (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       BIGINT NOT NULL REFERENCES Usuario(Id),
    Token           NVARCHAR(200) NOT NULL,
    DataExpiracao   DATETIME2 NOT NULL,
    Usado           BIT NOT NULL DEFAULT 0
);

-- =====================================================================
-- 2. ENDERECOS DO USUARIO (Nivel 2 de verificacao - ate 3 por usuario)
-- =====================================================================

CREATE TABLE Endereco (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       BIGINT NOT NULL REFERENCES Usuario(Id),
    Rotulo          NVARCHAR(50) NULL,        -- ex: "Minha casa", "Casa dos pais"
    Cep             NVARCHAR(10) NOT NULL,
    Rua             NVARCHAR(200) NOT NULL,
    Numero          NVARCHAR(20) NOT NULL,
    Complemento     NVARCHAR(100) NULL,
    Bairro          NVARCHAR(100) NOT NULL,
    Cidade          NVARCHAR(100) NOT NULL,
    Estado          CHAR(2) NOT NULL,
    DataCriacao     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Ativo           BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_Endereco_UsuarioId ON Endereco(UsuarioId);

-- Trigger simples para impor o limite de 3 enderecos ativos por usuario
-- (regra de negocio garantida tambem na camada de aplicacao, mas reforcada aqui)
GO
CREATE TRIGGER TRG_Endereco_LimiteTres
ON Endereco
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT UsuarioId FROM Endereco
        WHERE Ativo = 1 AND UsuarioId IN (SELECT UsuarioId FROM inserted)
        GROUP BY UsuarioId
        HAVING COUNT(*) > 3
    )
    BEGIN
        RAISERROR ('Limite de 3 enderecos ativos por usuario excedido.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- =====================================================================
-- 3. GRUPOS (Bairro / Condominio) - sem geografia, so texto
-- =====================================================================

CREATE TABLE Grupo (
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
    Nome                NVARCHAR(150) NOT NULL,
    Tipo                TINYINT NOT NULL,          -- 1=Bairro, 2=Condominio
    Bairro              NVARCHAR(100) NULL,
    Cidade              NVARCHAR(100) NOT NULL,
    Estado              CHAR(2) NOT NULL,
    EnderecoCompleto    NVARCHAR(300) NULL,        -- usado quando Tipo=Condominio
    CriadoPorUsuarioId  BIGINT NOT NULL REFERENCES Usuario(Id),
    DataCriacao         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Ativo               BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_Grupo_Cidade_Bairro ON Grupo(Cidade, Bairro);
CREATE INDEX IX_Grupo_Tipo ON Grupo(Tipo);

CREATE TABLE ConviteLink (
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
    GrupoId             BIGINT NOT NULL REFERENCES Grupo(Id),
    Token               NVARCHAR(100) NOT NULL,
    DataExpiracao       DATETIME2 NULL,
    LimiteUsos          INT NULL,
    UsosAtuais          INT NOT NULL DEFAULT 0,
    Ativo               BIT NOT NULL DEFAULT 1,

    CONSTRAINT UQ_ConviteLink_Token UNIQUE (Token)
);

-- Garante 1 link ativo por grupo
CREATE UNIQUE INDEX IX_ConviteLink_UnicoAtivoPorGrupo
    ON ConviteLink(GrupoId) WHERE Ativo = 1;

-- =====================================================================
-- 4. PARTICIPACAO EM GRUPO (substitui o antigo MembroGrupo)
-- =====================================================================
-- StatusParticipacao reflete o funil completo de entrada:
--   0 = AguardandoAprovacaoSolicitacao  (so canal Busca)
--   1 = AguardandoComprovante           (24h para enviar)
--   2 = AguardandoValidacaoAdmin        (sem prazo)
--   3 = Verificado
--   4 = Removido

CREATE TABLE ParticipacaoGrupo (
    Id                      BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId               BIGINT NOT NULL REFERENCES Usuario(Id),
    GrupoId                 BIGINT NOT NULL REFERENCES Grupo(Id),
    EnderecoId              BIGINT NULL REFERENCES Endereco(Id),  -- associado apos aprovacao/confirmacao
    Papel                   TINYINT NOT NULL DEFAULT 1,   -- 1=Membro, 2=Admin (MVP so tem esses 2)
    OrigemEntrada           TINYINT NOT NULL,             -- 1=Link, 2=Busca
    StatusParticipacao      TINYINT NOT NULL,
    PrazoComprovante        DATETIME2 NULL,                -- preenchido ao entrar em AguardandoComprovante
    MotivoRemocao           NVARCHAR(500) NULL,
    DataEntrada             DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacaoStatus   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Participacao_Usuario_Grupo UNIQUE (UsuarioId, GrupoId)
);

CREATE INDEX IX_Participacao_GrupoId_Status ON ParticipacaoGrupo(GrupoId, StatusParticipacao);
CREATE INDEX IX_Participacao_PrazoComprovante ON ParticipacaoGrupo(PrazoComprovante)
    WHERE StatusParticipacao = 1;  -- usado pelo job que remove automaticamente apos 24h

CREATE TABLE ComprovanteResidencia (
    Id                      BIGINT IDENTITY(1,1) PRIMARY KEY,
    ParticipacaoGrupoId     BIGINT NOT NULL REFERENCES ParticipacaoGrupo(Id),
    ArquivoUrl              NVARCHAR(500) NOT NULL,
    Status                  TINYINT NOT NULL DEFAULT 0,  -- 0=Pendente,1=Aprovado,2=Rejeitado
    MotivoRejeicao          NVARCHAR(500) NULL,
    RevisadoPorUsuarioId    BIGINT NULL REFERENCES Usuario(Id),
    DataEnvio               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DataRevisao             DATETIME2 NULL
);

CREATE INDEX IX_Comprovante_ParticipacaoId ON ComprovanteResidencia(ParticipacaoGrupoId);
CREATE INDEX IX_Comprovante_Status ON ComprovanteResidencia(Status);

-- =====================================================================
-- 5. POSTS / FEED
-- =====================================================================
-- Categoria: 1=Diario,2=CompraVenda,3=Alertas,4=AchadosPerdidos,
--            5=Eventos,6=Recomendacoes,7=Reclamacoes
-- NivelUrgencia (so para Alertas): 1=Baixo,2=Medio,3=Alto

CREATE TABLE Post (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    GrupoId         BIGINT NOT NULL REFERENCES Grupo(Id),
    AutorId         BIGINT NOT NULL REFERENCES Usuario(Id),
    Categoria       TINYINT NOT NULL,
    NivelUrgencia   TINYINT NULL,
    Titulo          NVARCHAR(200) NOT NULL,
    Conteudo        NVARCHAR(MAX) NOT NULL,
    Status          TINYINT NOT NULL DEFAULT 1,  -- 1=Ativo,2=OcultoAutomatico,3=RemovidoModeracao
    DataCriacao     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Ativo           BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_Post_Grupo_Categoria_Data ON Post(GrupoId, Categoria, DataCriacao DESC);

CREATE TABLE Comentario (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    PostId          BIGINT NOT NULL REFERENCES Post(Id),
    AutorId         BIGINT NOT NULL REFERENCES Usuario(Id),
    Conteudo        NVARCHAR(MAX) NOT NULL,
    DataCriacao     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Ativo           BIT NOT NULL DEFAULT 1
);

CREATE INDEX IX_Comentario_PostId ON Comentario(PostId);

-- Motivo: 1=Spam,2=ConteudoImproprio,3=DiscriminacaoOuPerfilamento,4=Outro
CREATE TABLE Denuncia (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    PostId          BIGINT NOT NULL REFERENCES Post(Id),
    UsuarioId       BIGINT NOT NULL REFERENCES Usuario(Id),
    Motivo          TINYINT NOT NULL,
    DataCriacao     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Denuncia_Post_Usuario UNIQUE (PostId, UsuarioId)
);

CREATE INDEX IX_Denuncia_PostId_Motivo ON Denuncia(PostId, Motivo);

-- =====================================================================
-- FIM DO SCHEMA (v2 - MVP atualizado)
-- =====================================================================
