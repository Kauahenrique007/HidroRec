namespace HidroRec.Backend.Domain.Entities;

public sealed class Organizacao : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Segmento { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public ICollection<AreaMonitorada> AreasMonitoradas { get; set; } = new List<AreaMonitorada>();

    public ICollection<AtivoMonitorado> AtivosMonitorados { get; set; } = new List<AtivoMonitorado>();
}
