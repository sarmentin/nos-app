namespace Nos.Application.Abstracoes;

public interface IPasswordHasher
{
    string GerarHash(string senha);
    bool Verificar(string senha, string hash);
}