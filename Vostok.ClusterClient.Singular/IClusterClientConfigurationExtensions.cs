using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Topology.CC;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Datacenters;
using Vostok.Metrics;
using Vostok.Singular.Core;
using Vostok.Singular.Core.PathPatterns.Idempotency;
using Vostok.Singular.Core.QualityMetrics;

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public static class IClusterClientConfigurationExtensions
    {
        private const string vostokClientName = "vostok";

        /// <summary>
        /// Sets up given ClusterClient configuration to send requests via Singular API gateway according to given <paramref name="settings"/>.
        /// </summary>
        public static void SetupSingular([NotNull] this IClusterClientConfiguration self, [NotNull] SingularClientSettings settings)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var clusterConfigClient = ClusterConfigClient.Default;

            if (settings.AlternativeClusterProvider != null)
                self.ClusterProvider = settings.AlternativeClusterProvider;
            else
                self.SetupClusterConfigTopology(clusterConfigClient, SingularConstants.CCTopologyName);

            self.RequestTransforms.Add(
                new AdHocRequestTransform(
                    request => request
                        .WithHeader(SingularHeaders.Environment, settings.TargetEnvironment)
                        .WithHeader(SingularHeaders.Service, settings.TargetService)));

            self.SetupWeighedReplicaOrdering(
                builder =>
                {
                    var datacenters = DatacentersProvider.Get();
                    builder.AddAdaptiveHealthModifierWithLinearDecay(TimeSpan.FromMinutes(5));
                    builder.SetupAvoidInactiveDatacentersWeightModifier(datacenters);
                    builder.SetupBoostLocalDatacentersWeightModifier(datacenters);
                });

            var forkingStrategy = Strategy.Forking(SingularClientConstants.ForkingStrategyParallelismLevel);
            var idempotencyIdentifier = IdempotencyIdentifierCache.Get(clusterConfigClient, settings.TargetEnvironment, settings.TargetService);
            self.DefaultRequestStrategy = new IdempotencySignBasedRequestStrategy(idempotencyIdentifier, Strategy.Sequential1, forkingStrategy);

            self.MaxReplicasUsedPerRequest = SingularClientConstants.ForkingStrategyParallelismLevel;

            self.TargetServiceName = $"{settings.TargetService} via {SingularConstants.ServiceName}";
            self.TargetEnvironment = settings.TargetEnvironment;

            InitializeMetricsProviderIfNeeded(self, settings.MetricContext, clusterConfigClient);
        }

        private static void InitializeMetricsProviderIfNeeded(IClusterClientConfiguration self, IMetricContext metricContext, IClusterConfigClient clusterConfigClient)
        {
            if (metricContext == null && MetricContextProvider.IsConfigured)
                metricContext = MetricContextProvider.Get();

            if (metricContext != null)
            {
                var environment = clusterConfigClient.Get(SingularConstants.EnvironmentNamePath)?.Value;
                if (environment == SingularConstants.ProdEnvironment || environment == SingularConstants.CloudEnvironment)
                {
                    var metricsProvider = MetricsProviderCache.Get(metricContext, environment, vostokClientName);
                    self.AddRequestModule(new MetricsModule(metricsProvider));
                }
            }
        }
    }
}