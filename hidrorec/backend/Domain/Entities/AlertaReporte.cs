namespace HidroRec.Backend.Domain.Entities;

public sealed class AlertaReporte
{
    public Guid AlertaId { get; set; }

    public Alerta Alerta { get; set; } = null!;

    public Guid ReporteId { get; set; }

    public Reporte Reporte { get; set; } = null!;
}
