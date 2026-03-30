using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Application.DTOs.Reportes;

public sealed class CreateReporteRequestDto
{
    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public NivelAgua NivelAgua { get; set; }

    public TipoOcorrencia TipoOcorrencia { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string EnderecoReferencia { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Regiao { get; set; } = string.Empty;

    public string NomeUsuario { get; set; } = string.Empty;

    public string ContatoUsuario { get; set; } = string.Empty;

    public string Observacoes { get; set; } = string.Empty;

    public string Fonte { get; set; } = "Colaborativa";

    public DateTime? DataOcorrencia { get; set; }

    public string? ImagemBase64 { get; set; }

    public string? ImagemNomeArquivo { get; set; }

    public string? ImagemContentType { get; set; }
}
