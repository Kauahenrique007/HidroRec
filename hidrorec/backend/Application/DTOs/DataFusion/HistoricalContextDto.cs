namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class HistoricalContextDto
{
    public int Reportes24h { get; set; }

    public int Alagamentos24h { get; set; }

    public int Confirmados24h { get; set; }

    public int Reportes7d { get; set; }

    public int Recorrencia30d { get; set; }

    public DateTime? LastOccurrenceAtUtc { get; set; }

    public decimal HistoricalPressure { get; set; }

    public string Summary { get; set; } = string.Empty;
}
