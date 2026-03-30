using FluentValidation;
using HidroRec.Backend.Application.DTOs.Reportes;

namespace HidroRec.Backend.Application.Validators;

public sealed class CreateReporteRequestDtoValidator : AbstractValidator<CreateReporteRequestDto>
{
    private static readonly HashSet<string> AllowedContentTypes = ["image/png", "image/jpeg", "image/webp"];

    public CreateReporteRequestDtoValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O titulo do reporte e obrigatorio.")
            .MaximumLength(120).WithMessage("O titulo deve ter no maximo 120 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descricao do reporte e obrigatoria.")
            .MinimumLength(10).WithMessage("Descreva melhor a ocorrencia.")
            .MaximumLength(600).WithMessage("A descricao deve ter no maximo 600 caracteres.");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Informe o bairro.");

        RuleFor(x => x.Regiao)
            .NotEmpty().WithMessage("Informe a regiao.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude invalida.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude invalida.");

        RuleFor(x => x.ImagemContentType)
            .Must(type => string.IsNullOrWhiteSpace(type) || AllowedContentTypes.Contains(type))
            .WithMessage("A imagem deve ser PNG, JPG ou WEBP.");

        RuleFor(x => x.ImagemBase64)
            .Must(base64 => string.IsNullOrWhiteSpace(base64) || base64.Length <= 8_000_000)
            .WithMessage("A imagem enviada e muito grande.");
    }
}
