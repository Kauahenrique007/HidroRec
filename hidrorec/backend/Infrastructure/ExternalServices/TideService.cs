using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Configurations;
using Microsoft.Extensions.Options;

namespace HidroRec.Backend.Infrastructure.ExternalServices;

public sealed class TideService(IOptions<ExternalDataOptions> externalOptions) : ITideService
{
    public Task<(decimal AlturaMetros, string Status, string Descricao)> GetCurrentTideAsync(CancellationToken cancellationToken)
    {
        var fallback = externalOptions.Value.TideFallback;
        return Task.FromResult((fallback.AlturaMetros, fallback.Status, fallback.Descricao));
    }
}
