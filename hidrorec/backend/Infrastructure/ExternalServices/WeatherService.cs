using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using Microsoft.Extensions.Options;

namespace HidroRec.Backend.Infrastructure.ExternalServices;

public sealed class WeatherService(
    HttpClient httpClient,
    IOptions<ExternalDataOptions> externalOptions,
    ILogger<WeatherService> logger) : IWeatherService
{
    public async Task<(decimal VolumeMmHora, string Status, string Descricao)> GetCurrentRainAsync(CancellationToken cancellationToken)
    {
        var options = externalOptions.Value.Apac;
        var municipality = Uri.EscapeDataString(options.RecifeMunicipalityName);
        var query = $"{options.RainMunicipality24hPath}?where=nm_mun%20%3D%20%27{municipality}%27&outFields=nm_mun,horas_24,ultima_leitura_data_hora&returnGeometry=false&f=json";
        var requestUri = new Uri(new Uri(options.BaseUrl.TrimEnd('/')), query);

        try
        {
            var response = await httpClient.GetFromJsonAsync<ApacFeatureCollection>(requestUri, cancellationToken);
            var feature = response?.Features?.FirstOrDefault();
            var rain24h = Convert.ToDecimal(feature?.Attributes?.Horas24 ?? 0d, CultureInfo.InvariantCulture);
            var readingAt = feature?.Attributes?.UltimaLeituraDataHora?.Trim();

            var status = ResolveRainStatus(rain24h);
            var description = string.IsNullOrWhiteSpace(readingAt)
                ? "Fonte APAC | acumulado oficial nas ultimas 24h para Recife."
                : $"Fonte APAC | acumulado oficial em 24h para Recife | leitura {readingAt}.";

            return (rain24h, status, description);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao consultar APAC. Aplicando fallback meteorologico local.");
            return (12m, "Moderada", "Fallback local ativo enquanto a APAC nao respondeu.");
        }
    }

    private static string ResolveRainStatus(decimal rain24h)
    {
        if (rain24h >= 100m) return "Extrema";
        if (rain24h >= 50m) return "Muito Forte";
        if (rain24h >= 30m) return "Forte";
        if (rain24h >= 10m) return "Moderada";
        if (rain24h >= 2m) return "Fraca";
        return "Baixa";
    }

    private sealed class ApacFeatureCollection
    {
        [JsonPropertyName("features")]
        public IReadOnlyCollection<ApacFeature> Features { get; set; } = Array.Empty<ApacFeature>();
    }

    private sealed class ApacFeature
    {
        [JsonPropertyName("attributes")]
        public ApacAttributes? Attributes { get; set; }
    }

    private sealed class ApacAttributes
    {
        [JsonPropertyName("horas_24")]
        public double? Horas24 { get; set; }

        [JsonPropertyName("ultima_leitura_data_hora")]
        public string? UltimaLeituraDataHora { get; set; }
    }
}
