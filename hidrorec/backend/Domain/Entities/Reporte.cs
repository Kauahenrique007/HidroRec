using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Domain.Entities;

public sealed class Reporte : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public NivelAgua NivelAgua { get; set; }

    public TipoOcorrencia TipoOcorrencia { get; set; }

    public SeveridadeReporte Severidade { get; set; }

    public StatusReporte Status { get; set; } = StatusReporte.Pendente;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string EnderecoReferencia { get; set; } = string.Empty;

    public int? BairroId { get; set; }

    public Bairro? Bairro { get; set; }

    public int? RegiaoId { get; set; }

    public Regiao? Regiao { get; set; }

    public string BairroNome { get; set; } = string.Empty;

    public string RegiaoNome { get; set; } = string.Empty;

    public Guid? UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public string NomeUsuario { get; set; } = string.Empty;

    public string ContatoUsuario { get; set; } = string.Empty;

    public string? CaminhoImagem { get; set; }

    public string Observacoes { get; set; } = string.Empty;

    public string Fonte { get; set; } = "Colaborativa";

    public DateTime DataOcorrencia { get; set; }

    public ICollection<HistoricoReporte> Historicos { get; set; } = new List<HistoricoReporte>();

    public ICollection<AlertaReporte> AlertaReportes { get; set; } = new List<AlertaReporte>();
}
