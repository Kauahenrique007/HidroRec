using FluentValidation;
using HidroRec.Backend.Application.DTOs.Usuarios;

namespace HidroRec.Backend.Application.Validators;

public sealed class UpdateOwnUsuarioRequestDtoValidator : AbstractValidator<UpdateOwnUsuarioRequestDto>
{
    public UpdateOwnUsuarioRequestDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome.")
            .MaximumLength(120).WithMessage("O nome deve ter no maximo 120 caracteres.");

        RuleFor(x => x.Telefone)
            .MaximumLength(40).WithMessage("O telefone deve ter no maximo 40 caracteres.");
    }
}
