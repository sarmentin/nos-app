using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nos.Application.Abstracoes;
using Nos.Application.Autenticacao.Cadastro;
using Nos.Infrastructure.Persistencia;
using Nos.Infrastructure.Repositorios;
using Nos.Infrastructure.Seguranca;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados
builder.Services.AddDbContext<NosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR - registra todos os Handlers do assembly do Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CadastrarUsuarioCommand).Assembly));

// FluentValidation - registra todos os Validators do assembly do Application
builder.Services.AddValidatorsFromAssembly(typeof(CadastrarUsuarioCommand).Assembly);

// Abstracoes -> Implementacoes concretas
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Autenticacao JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// (mantenha as linhas que ja existiam do template, tipo AddControllers, AddSwaggerGen, etc.)

var app = builder.Build();

// (mantenha o restante do pipeline que ja existia)

app.UseAuthentication();
app.UseAuthorization();

app.Run();