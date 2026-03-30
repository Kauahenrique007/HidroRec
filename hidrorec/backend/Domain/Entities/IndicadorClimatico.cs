namespace HidroRec.Backend.Domain.Entities;

public sealed class IndicadorClimatico : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Categoria { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public string Unidade { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateTime ReferenciaEm { get; set; }

    public int FonteDadoId { get; set; }

    public FonteDado FonteDado { get; set; } = null!;
}
