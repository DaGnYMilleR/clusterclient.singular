using System.Collections.Generic;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Collections;
using Vostok.Configuration;
using Vostok.Configuration.Sources.ClusterConfig;
using Vostok.Configuration.Sources.ClusterConfig.Converters;
using Vostok.Configuration.Sources.ClusterConfig.Legacy;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencyIdentifiersCache : IIdempotencyIdentifiersCache
    {
        private readonly List<ISettingsNodeConverter> customConverters = new List<ISettingsNodeConverter>
        {
            new LegacyCollectionConverter(typeof (NonIdempotencySignSettings).Name, "Signs")
        };
        private readonly CachingTransform<NonIdempotencySignsSettings, NonIdempotencySign[]> cache;

        public IdempotencyIdentifiersCache(IClusterConfigClient clusterConfigClient, string service)
        {
            var configurationProvider = new ConfigurationProvider(new ConfigurationProviderSettings());
            var sourceSettings = new ClusterConfigSourceSettings(clusterConfigClient, SingularConstants.GetNonIdempotencySigns(service)) {CustomConverters = customConverters};
            var source = new ClusterConfigSource(sourceSettings);
            configurationProvider.SetupSourceFor<NonIdempotencySignsSettings>(source);

            cache = new CachingTransform<NonIdempotencySignsSettings, NonIdempotencySign[]>(
                PreprocessSigns,
                configurationProvider.Get<NonIdempotencySignsSettings>);

            configurationProvider.Get<List<NonIdempotencySignSettings>>();
        }

        public NonIdempotencySign[] GetNonIdempotencySigns()
        {
            return cache.Get();
        }

        private static NonIdempotencySign[] PreprocessSigns(NonIdempotencySignsSettings nonIdempotencySignsSettings)
        {
            var signs = nonIdempotencySignsSettings.Signs;
            var processedSigns = new NonIdempotencySign[signs.Count];

            for (var i = 0; i < signs.Count; i++)
            {
                var sign = signs[i];
                processedSigns[i] = new NonIdempotencySign
                {
                    Method = sign.Method,
                    PathPattern = sign.PathPattern == null ? null : new Wildcard(sign.PathPattern)
                };
            }

            return processedSigns;
        }
    }
}