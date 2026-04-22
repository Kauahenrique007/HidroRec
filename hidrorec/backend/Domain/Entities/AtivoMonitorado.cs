namespace HidroRec.Backend.Domain.Entities;

public sealed class AtivoMonitorado : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public int OrganizacaoId { get; set; }

    public Organizacao Organizacao { get; set; } = null!;

    public int? AreaMonitoradaId { get; set; }

    public AreaMonitorada? AreaMonitorada { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Sensibilidade { get; set; }

    public string StatusOperacional { get; set; } = "Monitorado";

    public bool Ativo { get; set; } = true;
}
