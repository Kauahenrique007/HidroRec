using Microsoft.Extensions.Caching.Memory;

namespace HidroRec.Backend.Infrastructure.ExternalServices;

public sealed class ExternalDataCache(IMemoryCache memoryCache) : IExternalDataCache
{
    public bool TryGet<T>(string key, out T? value) where T : class
    {
        if (memoryCache.TryGetValue(key, out var cached) && cached is T typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }

    public void Set<T>(string key, T value) where T : class
    {
        memoryCache.Set(key, value, TimeSpan.FromHours(24));
    }
}
