using Nos.Application.Abstracoes;

namespace Nos.Infrastructure.Seguranca;

public class PasswordHasher : IPasswordHasher
{
    public string GerarHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}