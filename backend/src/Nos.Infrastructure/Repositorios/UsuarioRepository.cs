using Microsoft.EntityFrameworkCore;
using Nos.Application.Abstracoes;
using Nos.Domain.Entidades;
using Nos.Infrastructure.Persistencia;

namespace Nos.Infrastructure.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly NosDbContext _context;

    public UsuarioRepository(NosDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<Usuario?> ObterPorUsernameAsync(string username) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

    public async Task AdicionarAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);

    public async Task SalvarAlteracoesAsync() =>
        await _context.SaveChangesAsync();
}