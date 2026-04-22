namespace HidroRec.Backend.Infrastructure.ExternalServices;

public interface IExternalDataCache
{
    bool TryGet<T>(string key, out T? value) where T : class;

    void Set<T>(string key, T value) where T : class;
}
