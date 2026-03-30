using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class ReporteDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public NivelAgua NivelAgua { get; set; }

    public TipoOcorrencia TipoOcorrencia { get; set; }

    public SeveridadeReporte Severidade { get; set; }

    public StatusReporte Status { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string EnderecoReferencia { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Regiao { get; set; } = string.Empty;

    public string NomeUsuario { get; set; } = string.Empty;

    public string ContatoUsuario { get; set; } = string.Empty;

    public string? ImagemUrl { get; set; }

    public string Observacoes { get; set; } = string.Empty;

    public string Fonte { get; set; } = string.Empty;

    public DateTime DataOcorrencia { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime DataAtualizacao { get; set; }

    public IReadOnlyCollection<HistoricoReporteDto> Historico { get; set; } = Array.Empty<HistoricoReporteDto>();
}
