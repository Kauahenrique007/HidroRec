namespace HidroRec.Backend.Domain.Entities;

public sealed class Bairro : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int RegiaoId { get; set; }

    public Regiao Regiao { get; set; } = null!;

    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
}
