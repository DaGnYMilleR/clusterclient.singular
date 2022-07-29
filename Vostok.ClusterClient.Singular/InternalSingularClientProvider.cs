using System;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Topology.CC;
using Vostok.Clusterclient.Transport;
using Vostok.ClusterConfig.Client;
using Vostok.Datacenters;
using Vostok.Logging.Abstractions;
using Vostok.Singular.Core;

namespace Vostok.Clusterclient.Singular
{
    internal class InternalSingularClientProvider
    {
        private static readonly object Sync = new object();
        private static volatile IClusterClient singularClient;

        public static IClusterClient Get(ILog log = null, IClusterProvider alternativeCluster = null)
        {
            if (singularClient != null)
                return singularClient;

            lock (Sync)
            {
                // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
                singularClient = singularClient ?? Create(log, alternativeCluster);
                return singularClient;
            }
        }

        internal static IClusterClient Create(ILog log = null, IClusterProvider alternativeCluster = null)
        {
            var limitedLog = (log ?? LogProvider.Get()).WithMinimumLevel(LogLevel.Warn);
        
            return new Core.ClusterClient(limitedLog,
                configuration =>
                {
                    configuration.SetupUniversalTransport();
                    configuration.ClusterProvider = alternativeCluster ?? 
                                                    new ClusterConfigClusterProvider(ClusterConfigClient.Default, SingularConstants.CCTopologyName, log ?? LogProvider.Get());
                    configuration.SetupWeighedReplicaOrdering(
                        builder =>
                        {
                            var datacenters = DatacentersProvider.Get();
                            builder.AddRelativeWeightModifier(new RelativeWeightSettings());
                            builder.SetupAvoidInactiveDatacentersWeightModifier(datacenters);
                            builder.SetupBoostLocalDatacentersWeightModifier(datacenters);
                        });
                    configuration.Logging.LoggingMode = LoggingMode.SingleShortMessage;
                    configuration.TargetEnvironment = SingularConstants.DefaultZone;
                    configuration.TargetServiceName = SingularConstants.ServiceName;
                    configuration.DefaultRequestStrategy = Strategy.Forking(SingularClientConstants.ForkingStrategyParallelismLevel);
                    configuration.MaxReplicasUsedPerRequest = SingularClientConstants.ForkingStrategyParallelismLevel;
                });
        }

        public static bool TryConfigure(IClusterClient newClient)
        {
            if (newClient == null)
                throw new ArgumentNullException(nameof(newClient));

            if (singularClient != null)
                return false;

            lock (Sync)
            {
                if (singularClient != null)
                    return false;

                singularClient = newClient;
                return true;
            }
        }
    }
}