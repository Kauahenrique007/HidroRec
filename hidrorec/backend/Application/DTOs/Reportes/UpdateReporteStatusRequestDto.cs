using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class UpdateReporteStatusRequestDto
{
    public StatusReporte Status { get; set; }

    public string Observacao { get; set; } = string.Empty;
}
