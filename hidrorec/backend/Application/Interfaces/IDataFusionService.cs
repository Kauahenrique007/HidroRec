using HidroRec.Backend.Application.DTOs.DataFusion;

namespace HidroRec.Backend.Application.Interfaces;

public interface IDataFusionService
{
    Task<OperationalDataFusionSnapshotDto> GetSnapshotAsync(decimal? latitude, decimal? longitude, string? bairro, CancellationToken cancellationToken);
}
