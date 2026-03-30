namespace HidroRec.Backend.Configurations;

public sealed class ExternalDataOptions
{
    public const string SectionName = "ExternalData";

    public ApacOptions Apac { get; set; } = new();

    public TideFallbackOptions TideFallback { get; set; } = new();
}

public sealed class ApacOptions
{
    public string BaseUrl { get; set; } = "https://geoportal.apac.pe.gov.br";

    public string RainMunicipality24hPath { get; set; } = "/server/rest/services/met_volume_acumulado_chuva_municipio_24hs/MapServer/0/query";

    public string RecifeMunicipalityName { get; set; } = "Recife";
}

public sealed class TideFallbackOptions
{
    public decimal AlturaMetros { get; set; } = 2.3m;

    public string Status { get; set; } = "Alta";

    public string Descricao { get; set; } = "Referencia operacional local enquanto a integracao oceanografica oficial nao e conectada.";
}
