using MediatR;

namespace Nos.Application.Autenticacao.Cadastro;

public record CadastrarUsuarioCommand(
    string Nome,
    string Username,
    string Email,
    string Senha,
    string ConfirmarSenha,
    bool AceiteTermos
) : IRequest<CadastrarUsuarioResult>;

public record CadastrarUsuarioResult(long UsuarioId, string Token, string RefreshToken);
