namespace HidroRec.Backend.Domain.Entities;

public sealed class FonteDado : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public ICollection<IndicadorClimatico> IndicadoresClimaticos { get; set; } = new List<IndicadorClimatico>();
}
