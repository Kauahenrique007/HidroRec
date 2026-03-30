namespace HidroRec.Backend.Application.Interfaces;

public interface ITideService
{
    Task<(decimal AlturaMetros, string Status, string Descricao)> GetCurrentTideAsync(CancellationToken cancellationToken);
}
