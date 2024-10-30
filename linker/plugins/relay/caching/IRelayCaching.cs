﻿namespace linker.plugins.relay.caching
{
    public interface IRelayCaching
    {
        public ValueTask<bool> TryAdd<T>(string key, T value, int expired);
        public ValueTask<bool> TryGetValue<T>(string key, RelayCachingValue<T> wrap);
    }

    public sealed class RelayCachingValue<T>
    {
        public T Value { get; set; }
    }
}
