using FluentValidation;
using HidroRec.Backend.Application.DTOs.Auth;

namespace HidroRec.Backend.Application.Validators;

public sealed class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(120).WithMessage("O nome deve ter no maximo 120 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail valido.")
            .MaximumLength(180).WithMessage("O e-mail deve ter no maximo 180 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Informe a senha.")
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiuscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minuscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um numero.");

        RuleFor(x => x.Telefone)
            .MaximumLength(40).WithMessage("O telefone deve ter no maximo 40 caracteres.");
    }
}
