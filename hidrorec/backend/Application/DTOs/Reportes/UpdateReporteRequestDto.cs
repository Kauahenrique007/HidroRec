using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class UpdateReporteRequestDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public TipoOcorrencia TipoOcorrencia { get; set; }

    public string EnderecoReferencia { get; set; } = string.Empty;

    public string Observacoes { get; set; } = string.Empty;
}
