using FluentValidation;
using HidroRec.Backend.Application.DTOs.Usuarios;

namespace HidroRec.Backend.Application.Validators;

public sealed class UpdateUsuarioRequestDtoValidator : AbstractValidator<UpdateUsuarioRequestDto>
{
    public UpdateUsuarioRequestDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome do usuario.")
            .MaximumLength(120).WithMessage("O nome deve ter no maximo 120 caracteres.");

        RuleFor(x => x.Telefone)
            .MaximumLength(40).WithMessage("O telefone deve ter no maximo 40 caracteres.");

        RuleFor(x => x.PerfilId)
            .GreaterThan(0).WithMessage("Perfil invalido.");
    }
}
