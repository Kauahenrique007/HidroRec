namespace HidroRec.Backend.Configurations;

public sealed class ExternalDataOptions
{
    public const string SectionName = "ExternalData";

    public ApacOptions Apac { get; set; } = new();

    public OpenMeteoOptions OpenMeteo { get; set; } = new();

    public TideApiOptions TideApi { get; set; } = new();

    public DataFusionOptions DataFusion { get; set; } = new();

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

public sealed class OpenMeteoOptions
{
    public string BaseUrl { get; set; } = "https://api.open-meteo.com";

    public decimal Latitude { get; set; } = -8.0476m;

    public decimal Longitude { get; set; } = -34.8770m;

    public string Timezone { get; set; } = "America/Sao_Paulo";

    public int ForecastHours { get; set; } = 24;
}

public sealed class TideApiOptions
{
    public string BaseUrl { get; set; } = "https://tabuamare.devtu.qzz.io";

    public string HarborId { get; set; } = "pe02";
}

public sealed class DataFusionOptions
{
    public int MaxObservedRainAgeHours { get; set; } = 6;

    public int MaxForecastAgeHours { get; set; } = 3;

    public int MaxTideAgeHours { get; set; } = 24;

    public int MaxCacheAgeHours { get; set; } = 24;

    public decimal ApacReliability { get; set; } = 0.96m;

    public decimal OpenMeteoObservedReliability { get; set; } = 0.82m;

    public decimal OpenMeteoForecastReliability { get; set; } = 0.84m;

    public decimal TideTableReliability { get; set; } = 0.90m;
}
