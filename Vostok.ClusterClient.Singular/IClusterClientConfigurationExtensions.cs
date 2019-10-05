﻿using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Ordering.Weighed.Adaptive;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Strategies.DelayProviders;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Singular.NonIdempotency;
using Vostok.Clusterclient.Topology.CC;
using Vostok.ClusterConfig.Client;

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
                        .WithHeader(SingularConstants.EnvironmentHeader, settings.TargetEnvironment)
                        .WithHeader(SingularConstants.ServiceHeader, settings.TargetService)));

            self.SetupWeighedReplicaOrdering(
                builder =>
                {
                    builder.AddAdaptiveHealthModifierWithLinearDecay(TuningPolicies.ByResponseVerdict, TimeSpan.FromMinutes(5));
                });

            if (settings.AlternativeDefaultRequestStrategy != null)
            {
                self.DefaultRequestStrategy = settings.AlternativeDefaultRequestStrategy;
            }
            else
            {
                var sequential1Strategy = Strategy.Sequential1;
                var forkingStrategy = new ForkingRequestStrategy(new EqualDelaysProvider(SingularConstants.ForkingStrategyParallelismLevel), SingularConstants.ForkingStrategyParallelismLevel);
                var idempotencyIdentifiersCache = new IdempotencyIdentifiersCache(clusterConfigClient, settings.TargetService);
                var idempotencyIdentifier = new IdempotencyIdentifier(idempotencyIdentifiersCache);
                self.DefaultRequestStrategy = new IdempotencySingBasedRequestStrategy(idempotencyIdentifier, sequential1Strategy, forkingStrategy);
            }

            self.MaxReplicasUsedPerRequest = 3;

            self.TargetServiceName = SingularConstants.SingularServiceName;
        }
    }
}