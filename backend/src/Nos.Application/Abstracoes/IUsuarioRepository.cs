using Nos.Domain.Entidades;

namespace Nos.Application.Abstracoes;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorUsernameAsync(string username);
    Task AdicionarAsync(Usuario usuario);
    Task SalvarAlteracoesAsync();
}