using MediatR;
using Nos.Application.Abstracoes;
using Nos.Domain.Entidades;

namespace Nos.Application.Autenticacao.Cadastro;

public class CadastrarUsuarioHandler : IRequestHandler<CadastrarUsuarioCommand, CadastrarUsuarioResult>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public CadastrarUsuarioHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<CadastrarUsuarioResult> Handle(CadastrarUsuarioCommand request, CancellationToken cancellationToken)
    {
        if (await _usuarioRepository.ObterPorEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("Este e-mail já está cadastrado.");

        if (await _usuarioRepository.ObterPorUsernameAsync(request.Username) is not null)
            throw new InvalidOperationException("Este username já está em uso.");

        var senhaHash = _passwordHasher.GerarHash(request.Senha);

        var usuario = Usuario.Criar(request.Nome, request.Username, request.Email, senhaHash, request.AceiteTermos);

        await _usuarioRepository.AdicionarAsync(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();

        var (token, refreshToken) = _tokenService.GerarTokens(usuario);

        return new CadastrarUsuarioResult(usuario.Id, token, refreshToken);
    }
}