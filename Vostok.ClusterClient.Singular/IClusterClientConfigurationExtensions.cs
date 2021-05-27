using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Singular.ServiceMesh;
using Vostok.Clusterclient.Topology.CC;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Datacenters;
using Vostok.Metrics;
using Vostok.Singular.Core;
using Vostok.Singular.Core.PathPatterns.Idempotency;
using Vostok.Singular.Core.QualityMetrics;

#nullable enable

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public static class IClusterClientConfigurationExtensions
    {
        // todo (deniaa, 03.02.2021): Use a different "metrics context name" for SLO metrics if we are not just a simple SingularClient, but a ServiceMeshClient AND we have a local Singular installed.
        private const string SloMetricsClientName = "vostok";

        /// <summary>
        /// Sets up given ClusterClient configuration to send requests via Singular API gateway according to given <paramref name="settings"/>.
        /// </summary>
        public static void SetupSingular(this IClusterClientConfiguration configuration, SingularClientSettings settings)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            configuration.RequestTransforms.Add(
                new AdHocRequestTransform(
                    request => request
                        .WithHeader(SingularHeaders.Environment, settings.TargetEnvironment)
                        .WithHeader(SingularHeaders.Service, settings.TargetService)));

            configuration.TargetEnvironment = settings.TargetEnvironment;
            configuration.TargetServiceName = ServiceMeshEnvironmentInfo.UseLocalSingular
                ? $"{settings.TargetService} via ServiceMesh"
                : $"{settings.TargetService} via {SingularConstants.ServiceName}";

            var clusterConfigClient = ClusterConfigClient.Default;

            configuration.ClusterProvider = settings.AlternativeClusterProvider ??
                                            new ClusterConfigClusterProvider(clusterConfigClient, SingularConstants.CCTopologyName, configuration.Log);

            configuration.SetupWeighedReplicaOrdering(
                builder =>
                {
                    var datacenters = DatacentersProvider.Get();
                    builder.AddRelativeWeightModifier(new RelativeWeightSettings());
                    builder.SetupAvoidInactiveDatacentersWeightModifier(datacenters);
                    builder.SetupBoostLocalDatacentersWeightModifier(datacenters);
                });

            var forkingStrategy = Strategy.Forking(SingularClientConstants.ForkingStrategyParallelismLevel);
            var idempotencyIdentifier = IdempotencyIdentifierCache.Get(clusterConfigClient, settings.TargetEnvironment, settings.TargetService);
            configuration.DefaultRequestStrategy = new IdempotencySignBasedRequestStrategy(idempotencyIdentifier, Strategy.Sequential1, forkingStrategy);

            configuration.MaxReplicasUsedPerRequest = SingularClientConstants.ForkingStrategyParallelismLevel;

            if (ServiceMeshEnvironmentInfo.UseLocalSingular)
            {
                var serviceMeshRequestModule = new ServiceMeshRequestModule(configuration.Log, idempotencyIdentifier);
                configuration.AddRequestModule(serviceMeshRequestModule, RequestModule.RequestExecution);
            }

            InitializeMetricsProviderIfNeeded(configuration, settings.MetricContext, clusterConfigClient);
        }

        private static void InitializeMetricsProviderIfNeeded(IClusterClientConfiguration configuration, IMetricContext? metricContext, IClusterConfigClient clusterConfigClient)
        {
            if (metricContext == null && MetricContextProvider.IsConfigured)
                metricContext = MetricContextProvider.Get();

            if (metricContext != null)
            {
                var environment = clusterConfigClient.Get(SingularConstants.EnvironmentNamePath)?.Value;
                if (environment == SingularConstants.ProdEnvironment || environment == SingularConstants.CloudEnvironment)
                {
                    var metricsProvider = MetricsProviderCache.Get(metricContext, environment, SloMetricsClientName);
                    configuration.AddRequestModule(new MetricsModule(metricsProvider));
                }
            }
        }
    }
}