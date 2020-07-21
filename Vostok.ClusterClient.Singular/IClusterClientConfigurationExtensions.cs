using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Strategies.DelayProviders;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Singular.NonIdempotency;
using Vostok.Clusterclient.Topology.CC;
using Vostok.ClusterConfig.Client;
using Vostok.Datacenters;
using Vostok.Singular.Core;
using Vostok.Metrics;
using Vostok.Singular.Core.PathPatterns.Idempotency;
using Vostok.Singular.Core.QualityMetrics;

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public static class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Sets up given ClusterClient configuration to send requests via Singular API gateway according to given <paramref name="settings"/>.
        /// </summary>
        public static void SetupSingular([NotNull] this IClusterClientConfiguration self, [NotNull] SingularClientSettings settings)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var clusterConfigClient = settings.ClusterConfigClient ?? ClusterConfigClient.Default;

            if (settings.AlternativeClusterProvider != null)
            {
                self.ClusterProvider = settings.AlternativeClusterProvider;
            }
            else
            {
                self.SetupClusterConfigTopology(clusterConfigClient, settings.ClusterConfigTopologyPath);
            }

            self.RequestTransforms.Add(
                new AdHocRequestTransform(
                    request => request
                        .WithHeader(SingularHeaders.Environment, settings.TargetEnvironment)
                        .WithHeader(SingularHeaders.Service, settings.TargetService)));

            self.SetupWeighedReplicaOrdering(
                builder =>
                {
                    builder.AddAdaptiveHealthModifierWithLinearDecay(TimeSpan.FromMinutes(5));
                    builder.SetupAvoidInactiveDatacentersWeightModifier(DatacentersProvider.Get());
                    builder.SetupBoostLocalDatacentersWeightModifier(DatacentersProvider.Get());
                });

            if (settings.AlternativeDefaultRequestStrategy != null)
            {
                self.DefaultRequestStrategy = settings.AlternativeDefaultRequestStrategy;
            }
            else if (settings.DisableStrategyBasedOnBackendSettings || settings.AlternativeClusterProvider != null)
            {
                self.DefaultRequestStrategy = Strategy.Sequential1;
            }
            else
            {
                var sequential1Strategy = Strategy.Sequential1;
                var forkingStrategy = new ForkingRequestStrategy(new EqualDelaysProvider(SingularClientConstants.ForkingStrategyParallelismLevel), SingularClientConstants.ForkingStrategyParallelismLevel);
                var idempotencyIdentifier = IdempotencyIdentifierCache.Get(settings.TargetService);
                self.DefaultRequestStrategy = new IdempotencySingBasedRequestStrategy(idempotencyIdentifier, sequential1Strategy, forkingStrategy);
            }

            self.MaxReplicasUsedPerRequest = 3;

            self.TargetServiceName = $"{settings.TargetService} via {SingularConstants.ServiceName}";
            self.TargetEnvironment = settings.TargetEnvironment;

            InitializeMetricsProviderIfNeeded(self, settings.MetricContext);
        }

        private static void InitializeMetricsProviderIfNeeded(IClusterClientConfiguration self, IMetricContext metricContext)
        {
            if (metricContext == null && MetricContextProvider.IsConfigured)
                metricContext = MetricContextProvider.Get();

            if (metricContext != null)
            {
                var environment = ClusterConfigClient.Default.Get(SingularConstants.EnvironmentNamePath)?.Value;
                if (environment == SingularConstants.ProdEnvironment || environment == SingularConstants.CloudEnvironment)
                {
                    var metricsProvider = MetricsProviderCache.Get(metricContext, environment, VostokClientName);
                    self.AddRequestModule(new MetricsModule(metricsProvider));
                }
            }
        }

        private const string VostokClientName = "vostok";
    }
}