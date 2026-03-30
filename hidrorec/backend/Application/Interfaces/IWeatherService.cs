namespace HidroRec.Backend.Application.Interfaces;

public interface IWeatherService
{
    Task<(decimal VolumeMmHora, string Status, string Descricao)> GetCurrentRainAsync(CancellationToken cancellationToken);
}
