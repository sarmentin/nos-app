using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nos.Application.Abstracoes;
using Nos.Domain.Entidades;

namespace Nos.Infrastructure.Seguranca;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, string RefreshToken) GerarTokens(Usuario usuario)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("username", usuario.Username)
        };

        var expiracaoMinutos = int.Parse(_configuration["Jwt:ExpiracaoMinutos"]!);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiracaoMinutos),
            signingCredentials: credenciais
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GerarRefreshTokenAleatorio();

        return (tokenString, refreshToken);
    }

    private static string GerarRefreshTokenAleatorio()
    {
        var bytesAleatorios = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytesAleatorios);
    }
}