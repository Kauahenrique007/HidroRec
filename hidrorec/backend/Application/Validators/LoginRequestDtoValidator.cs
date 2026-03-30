using FluentValidation;
using HidroRec.Backend.Application.DTOs.Auth;

namespace HidroRec.Backend.Application.Validators;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail valido.")
            .MaximumLength(180).WithMessage("O e-mail deve ter no maximo 180 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Informe a senha.")
            .MinimumLength(8).WithMessage("A senha informada e invalida.");
    }
}
