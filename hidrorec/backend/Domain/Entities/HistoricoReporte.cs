using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Domain.Entities;

public sealed class HistoricoReporte : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReporteId { get; set; }

    public Reporte Reporte { get; set; } = null!;

    public StatusReporte StatusAnterior { get; set; }

    public StatusReporte StatusNovo { get; set; }

    public string Observacao { get; set; } = string.Empty;

    public Guid? AlteradoPorUsuarioId { get; set; }

    public Usuario? AlteradoPorUsuario { get; set; }

    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
}
