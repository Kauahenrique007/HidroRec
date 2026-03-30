using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class ReporteAdminItemDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Regiao { get; set; } = string.Empty;

    public TipoOcorrencia TipoOcorrencia { get; set; }

    public SeveridadeReporte Severidade { get; set; }

    public StatusReporte Status { get; set; }

    public DateTime DataOcorrencia { get; set; }
}
