# Contrato de API — Bairro Conecta (MVP)

Documento de especificação (Fase 4). Não é implementação — é o contrato entre
backend e frontend, servindo de referência para as Fases 6 e 7.

Convenções gerais:
- Todas as rotas autenticadas exigem header `Authorization: Bearer {token}`
- Respostas de erro seguem o formato: `{ "codigo": "string", "mensagem": "string" }`
- Datas em ISO 8601, UTC
- Paginação (onde aplicável): query params `pagina` e `tamanhoPagina`, resposta com `{ itens: [], totalItens, paginaAtual }`

---

## 1. Autenticação

### `POST /api/auth/cadastro`
**Tela**: Cadastro
**Request**: `{ nome, username, email, senha, confirmarSenha, aceiteTermos: bool }`
**Response 201**: `{ usuarioId, token, refreshToken }`
**Erros**: `EMAIL_JA_CADASTRADO`, `USERNAME_JA_CADASTRADO`, `SENHA_FRACA`, `TERMOS_NAO_ACEITOS`

### `POST /api/auth/login`
**Tela**: Login
**Request**: `{ email, senha }`
**Response 200**: `{ usuarioId, token, refreshToken }`
**Erros**: `CREDENCIAIS_INVALIDAS` (genérico, nunca especifica se é o e-mail ou a senha)
**Nota de segurança**: rate limiting por IP/e-mail para mitigar força bruta

### `POST /api/auth/refresh`
**Request**: `{ refreshToken }`
**Response 200**: `{ token, refreshToken }`
**Erros**: `REFRESH_TOKEN_INVALIDO_OU_EXPIRADO`

### `POST /api/auth/esqueci-senha`
**Tela**: Esqueci Minha Senha
**Request**: `{ email }`
**Response 200**: sempre `{ mensagem: "Se o e-mail existir, um link foi enviado." }` — mesma resposta exista ou não o e-mail (evita enumeração de contas)

### `POST /api/auth/redefinir-senha`
**Tela**: Redefinir Senha
**Request**: `{ tokenRedefinicao, novaSenha, confirmarSenha }`
**Response 200**: `{ mensagem: "Senha redefinida com sucesso." }`
**Erros**: `TOKEN_INVALIDO_OU_EXPIRADO`, `SENHA_FRACA`

---

## 2. Usuário / Perfil

### `GET /api/usuarios/me`
**Tela**: Perfil do Usuário
**Response 200**: `{ nome, username, email, enderecos: [...], grupos: [{ grupoId, nome, tipo, statusParticipacao }] }`

### `GET /api/usuarios/me/enderecos`
**Tela**: Meus Endereços
**Response 200**: `[{ enderecoId, rotulo, cep, rua, numero, complemento, bairro, cidade, estado }]`

### `POST /api/usuarios/me/enderecos`
**Tela**: Meus Endereços (adicionar)
**Request**: `{ rotulo, cep, rua, numero, complemento, bairro, cidade, estado }`
**Response 201**: `{ enderecoId }`
**Erros**: `LIMITE_DE_ENDERECOS_ATINGIDO` (máx. 3), `CAMPOS_OBRIGATORIOS_FALTANDO`

### `DELETE /api/usuarios/me/enderecos/{enderecoId}`
**Erros**: `ENDERECO_EM_USO_POR_PARTICIPACAO_ATIVA` (não permite excluir endereço vinculado a um grupo ativo)

---

## 3. Grupos — Busca e Criação

### `GET /api/grupos/buscar?texto={texto}`
**Tela**: Buscar Grupo
**Response 200**: `[{ grupoId, nome, tipo, regiaoAproximada }]` (busca textual simples por nome/bairro/cidade/CEP — sem geocoding)
**Response 200 vazio**: `[]` (frontend trata como estado vazio)

### `GET /api/grupos/{grupoId}`
**Tela**: Detalhe do Grupo Encontrado
**Response 200**: `{ grupoId, nome, tipo, descricao, regiaoAproximada }`
**Nota**: nunca retorna lista de membros nem endereço exato

### `POST /api/grupos`
**Tela**: Criar Grupo
**Request**: `{ tipo: "Bairro" | "Condominio", nome, bairro?, cidade, estado, enderecoCompleto? }` (campos condicionais conforme tipo)
**Response 201**: `{ grupoId, linkConvite }`
**Erros**: `GRUPO_SIMILAR_JA_EXISTE` (retorna também os grupos similares encontrados, para o frontend sugerir Buscar Grupo em vez de criar)

### `GET /api/grupos/{grupoId}/convite-link`
**Tela**: Grupo Criado com Sucesso (e reexibição posterior, se o Admin quiser ver o link de novo)
**Response 200**: `{ token, url, ativo, dataExpiracao, limiteUsos, usosAtuais }`

### `POST /api/grupos/{grupoId}/convite-link/revogar`
**Response 200**: `{ mensagem: "Link revogado." }`

---

## 4. Entrada em Grupo

### `POST /api/grupos/{grupoId}/solicitar-entrada`
**Tela**: Detalhe do Grupo Encontrado → botão "Solicitar entrada" (Canal Busca)
**Response 201**: `{ solicitacaoId, status: "AguardandoAprovacao" }`
**Erros**: `USUARIO_JA_TEM_SOLICITACAO_PENDENTE`, `USUARIO_JA_E_MEMBRO`

### `GET /api/convites/{token}`
**Tela**: Confirmação de Entrada via Link
**Response 200**: `{ grupoId, nome, tipo, descricao }`
**Erros**: `TOKEN_INVALIDO`, `TOKEN_EXPIRADO`, `LIMITE_DE_USOS_ATINGIDO`

### `POST /api/convites/{token}/confirmar`
**Tela**: Confirmação de Entrada via Link → botão "Confirmar entrada"
**Request**: `{ enderecoId }` (endereço já escolhido/cadastrado na etapa anterior)
**Response 201**: `{ participacaoId, status: "AguardandoComprovante", prazoExpiracao }`
**Nota**: entrada provisória imediata, sem aprovação prévia — diferente do canal Busca

### `POST /api/grupos/{grupoId}/participacao/enderecos`
**Tela**: Meus Endereços (associar endereço ao grupo, para o canal Busca depois da aprovação da solicitação)
**Request**: `{ enderecoId }`
**Response 200**: `{ participacaoId, status: "AguardandoComprovante", prazoExpiracao }`

### `GET /api/grupos/{grupoId}/participacao/status`
**Tela**: Status de Participação
**Response 200**: `{ status, prazoExpiracao?, motivoRemocao? }`
Valores de `status`: `AguardandoAprovacaoSolicitacao`, `AguardandoComprovante`, `AguardandoValidacaoAdmin`, `Verificado`, `Removido`

### `POST /api/grupos/{grupoId}/participacao/comprovante`
**Tela**: Upload de Comprovante (multipart/form-data)
**Request**: arquivo (imagem ou PDF)
**Response 200**: `{ status: "AguardandoValidacaoAdmin" }` (prazo de 24h é encerrado neste momento)
**Erros**: `PRAZO_EXPIRADO` (24h já passaram — usuário já teria sido removido automaticamente antes disso ser possível), `TIPO_DE_ARQUIVO_INVALIDO`, `ARQUIVO_MUITO_GRANDE`

---

## 5. Administração

### `GET /api/grupos/{grupoId}/admin/solicitacoes`
**Tela**: Painel do Admin — aba Solicitações
**Response 200**: `[{ solicitacaoId, usuario: { nome, username }, dataSolicitacao }]`

### `POST /api/grupos/{grupoId}/admin/solicitacoes/{solicitacaoId}/aprovar`
**Response 200**: `{ mensagem: "Solicitação aprovada." }` → participação do usuário muda para `AguardandoComprovante` (dispara o prazo de 24h)

### `POST /api/grupos/{grupoId}/admin/solicitacoes/{solicitacaoId}/rejeitar`
**Request**: `{ motivo }`
**Response 200**: `{ mensagem: "Solicitação rejeitada." }`

### `GET /api/grupos/{grupoId}/admin/comprovantes`
**Tela**: Painel do Admin — aba Comprovantes
**Response 200**: `[{ comprovanteId, usuario: { nome, username }, dataEnvio }]`

### `GET /api/grupos/{grupoId}/admin/comprovantes/{comprovanteId}`
**Tela**: Detalhe do Comprovante
**Response 200**: `{ arquivoUrl, usuario: { nome, enderecoDeclarado }, dataEnvio }`

### `POST /api/grupos/{grupoId}/admin/comprovantes/{comprovanteId}/aprovar`
**Response 200**: `{ mensagem: "Comprovante aprovado." }` → participação vira `Verificado`

### `POST /api/grupos/{grupoId}/admin/comprovantes/{comprovanteId}/rejeitar`
**Request**: `{ motivo }` (obrigatório — usuário recebe essa mensagem na notificação)
**Response 200**: `{ mensagem: "Comprovante rejeitado." }` → participação vira `Removido`, motivo registrado

---

## 6. Feed / Posts

### `GET /api/grupos/{grupoId}/posts?categoria={categoria}&pagina={n}`
**Tela**: Feed do Grupo
**Response 200**: `{ itens: [{ postId, categoria, nivelUrgencia?, titulo, trecho, autor, dataCriacao, destaque: bool }], totalItens, paginaAtual }`
**Nota**: só retorna posts com status `Ativo` (oculto/removido não aparece para membros comuns)

### `POST /api/grupos/{grupoId}/posts`
**Tela**: Criar Post
**Request**: `{ categoria, nivelUrgencia?, titulo, conteudo }`
**Erros**: `URGENCIA_OBRIGATORIA_PARA_ALERTA`, `DESCRICAO_MUITO_CURTA_PARA_ALERTA`
**Response 201**: `{ postId }`

### `GET /api/posts/{postId}`
**Tela**: Detalhe do Post
**Response 200**: `{ postId, categoria, nivelUrgencia?, titulo, conteudo, autor, dataCriacao, comentarios: [...] }`

### `POST /api/posts/{postId}/comentarios`
**Request**: `{ conteudo }`
**Response 201**: `{ comentarioId }`

### `POST /api/posts/{postId}/denuncias`
**Tela**: Detalhe do Post — modal de denúncia
**Request**: `{ motivo }` (inclui valor específico `"DiscriminacaoOuPerfilamento"`)
**Response 201**: `{ mensagem: "Denúncia registrada." }`
**Erros**: `USUARIO_JA_DENUNCIOU_ESTE_POST`
**Nota**: se atingir o threshold (5 gerais, ou 2 com motivo de discriminação), o post muda automaticamente para `OcultoAutomatico` — não requer chamada adicional do frontend

---

## 7. Home / Seleção de Grupo

### `GET /api/usuarios/me/grupos`
**Tela**: Home / Seleção de Grupo
**Response 200**: `[{ grupoId, nome, tipo, statusParticipacao }]`
**Nota**: mesmo endpoint de dado já coberto em `GET /api/usuarios/me`, mas exposto separado por ser o carregamento inicial mais frequente do app (tela de entrada) — candidato natural a cache

---

## Resumo por tela (rastreabilidade)

| Tela | Endpoints principais |
|---|---|
| Login | `POST /auth/login` |
| Cadastro | `POST /auth/cadastro` |
| Esqueci Minha Senha | `POST /auth/esqueci-senha` |
| Redefinir Senha | `POST /auth/redefinir-senha` |
| Buscar Grupo | `GET /grupos/buscar` |
| Detalhe do Grupo Encontrado | `GET /grupos/{id}`, `POST /grupos/{id}/solicitar-entrada` |
| Confirmação de Entrada via Link | `GET /convites/{token}`, `POST /convites/{token}/confirmar` |
| Meus Endereços | `GET/POST /usuarios/me/enderecos`, `POST /grupos/{id}/participacao/enderecos` |
| Upload de Comprovante | `POST /grupos/{id}/participacao/comprovante` |
| Status de Participação | `GET /grupos/{id}/participacao/status` |
| Criar Grupo | `POST /grupos` |
| Grupo Criado com Sucesso | `GET /grupos/{id}/convite-link` |
| Painel do Admin | `GET /grupos/{id}/admin/solicitacoes`, `GET /grupos/{id}/admin/comprovantes` |
| Detalhe do Comprovante | `GET/POST .../comprovantes/{id}/aprovar\|rejeitar` |
| Feed do Grupo | `GET /grupos/{id}/posts` |
| Criar Post | `POST /grupos/{id}/posts` |
| Detalhe do Post | `GET /posts/{id}`, `POST /posts/{id}/comentarios`, `POST /posts/{id}/denuncias` |
| Home / Seleção de Grupo | `GET /usuarios/me/grupos` |
| Perfil do Usuário | `GET /usuarios/me` |
