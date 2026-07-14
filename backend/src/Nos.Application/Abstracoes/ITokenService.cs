using Nos.Domain.Entidades;

namespace Nos.Application.Abstracoes;

public interface ITokenService
{
    (string Token, string RefreshToken) GerarTokens(Usuario usuario);
}