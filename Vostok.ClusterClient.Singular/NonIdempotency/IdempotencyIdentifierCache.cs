using System;
using System.Collections.Concurrent;
using Vostok.Clusterclient.Singular.NonIdempotency.Identifier;
using Vostok.Configuration;
using Vostok.Configuration.Logging;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal static class IdempotencyIdentifierCache
    {
        private static readonly ConcurrentDictionary<string, Lazy<IIdempotencyIdentifier>> Cache = new ConcurrentDictionary<string, Lazy<IIdempotencyIdentifier>>();

        static IdempotencyIdentifierCache()
        {
            var providerSettings = new ConfigurationProviderSettings()
                .WithErrorLogging(LogProvider.Get())
                .WithSettingsLogging(LogProvider.Get());

            var provider = new ConfigurationProvider(providerSettings);

            if(!ConfigurationProvider.TrySetDefault(provider))
                provider.Dispose();
        }

        public static IIdempotencyIdentifier  Get(string serviceName)
        {
            return Cache.GetOrAdd(serviceName, s => new Lazy<IIdempotencyIdentifier>(() => Create(s))).Value;
        }

        private static IIdempotencyIdentifier Create(string serviceName)
        {
            var idempotencySignsCache = new IdempotencySignsCache(new IdempotencySignsProvider(serviceName));
            return new IdempotencyIdentifier(idempotencySignsCache);
        }
    }
}