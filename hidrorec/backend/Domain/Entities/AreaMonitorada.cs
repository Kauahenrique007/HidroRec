namespace HidroRec.Backend.Domain.Entities;

public sealed class AreaMonitorada : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int OrganizacaoId { get; set; }

    public Organizacao Organizacao { get; set; } = null!;

    public int? BairroId { get; set; }

    public Bairro? Bairro { get; set; }

    public int? RegiaoId { get; set; }

    public Regiao? Regiao { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Vulnerabilidade { get; set; }

    public int CriticidadeOperacional { get; set; }

    public string Cobertura { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public ICollection<AtivoMonitorado> AtivosMonitorados { get; set; } = new List<AtivoMonitorado>();

    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}
