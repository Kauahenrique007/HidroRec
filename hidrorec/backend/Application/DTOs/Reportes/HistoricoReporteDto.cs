using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class HistoricoReporteDto
{
    public StatusReporte StatusAnterior { get; set; }

    public StatusReporte StatusNovo { get; set; }

    public string Observacao { get; set; } = string.Empty;

    public string AlteradoPor { get; set; } = string.Empty;

    public DateTime DataAlteracao { get; set; }
}
