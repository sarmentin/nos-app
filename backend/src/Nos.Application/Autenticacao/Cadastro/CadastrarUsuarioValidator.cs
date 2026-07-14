using FluentValidation;

namespace Nos.Application.Autenticacao.Cadastro;

public class CadastrarUsuarioValidator : AbstractValidator<CadastrarUsuarioCommand>
{
    public CadastrarUsuarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Username)
            .NotEmpty().MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("Username só pode conter letras, números, ponto e underline.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Senha)
            .NotEmpty().MinimumLength(8)
            .WithMessage("A senha precisa ter no mínimo 8 caracteres.");

        RuleFor(x => x.ConfirmarSenha)
            .Equal(x => x.Senha).WithMessage("As senhas não coincidem.");

        RuleFor(x => x.AceiteTermos)
            .Equal(true).WithMessage("É necessário aceitar os Termos de Uso.");
    }
}