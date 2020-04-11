using System.Collections.Generic;
using Vostok.Clusterclient.Singular.NonIdempotency.Settings;
using Vostok.ClusterConfig.Client;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources.ClusterConfig;
using Vostok.Configuration.Sources.Json;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencySignsProvider : IIdempotencySignsProvider
    {
        private readonly IConfigurationSource source;
        private readonly string servicePath;
        private static readonly NonIdempotencySignsSettings EmptySigns = new NonIdempotencySignsSettings {Signs = new List<NonIdempotencySignSettings>(0)};

        public IdempotencySignsProvider(string serviceName, string configurationPathPrefix = SingularConstants.ServicesConfigurationNamePrefix)
        {
            servicePath = $"{configurationPathPrefix}{serviceName}.json";
            source = new ClusterConfigSource(
                new ClusterConfigSourceSettings(ClusterConfigClient.Default, servicePath)
                {
                    ValuesParser = (value, path) => JsonConfigurationParser.Parse(value)
                });
        }

        public NonIdempotencySignsSettings Get()
        {
            if (ClusterConfigClient.Default.Get(servicePath) == null)
                return EmptySigns;

            return ConfigurationProvider.Default.Get<NonIdempotencySignsSettings>(source);
        }
    }
}