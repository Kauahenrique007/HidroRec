using FluentValidation;
using HidroRec.Backend.Application.DTOs.Reportes;

namespace HidroRec.Backend.Application.Validators;

public sealed class UpdateReporteStatusRequestDtoValidator : AbstractValidator<UpdateReporteStatusRequestDto>
{
    public UpdateReporteStatusRequestDtoValidator()
    {
        RuleFor(x => x.Observacao)
            .NotEmpty().WithMessage("Informe o motivo da alteracao de status.")
            .MinimumLength(4).WithMessage("A observacao deve ter pelo menos 4 caracteres.")
            .MaximumLength(300).WithMessage("A observacao deve ter no maximo 300 caracteres.");
    }
}
