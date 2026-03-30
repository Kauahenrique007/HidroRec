using HidroRec.Backend.Application.DTOs.Previsoes;

namespace HidroRec.Backend.Application.Interfaces;

public interface IPrevisaoService
{
    Task<PrevisaoResumoDto> GetResumoAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PrevisaoCardDto>> GetChuvaAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PrevisaoCardDto>> GetMareAsync(CancellationToken cancellationToken);
}
