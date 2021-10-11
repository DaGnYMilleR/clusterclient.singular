using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Strategies;
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

        public static IClusterClient Get(ILog log = null)
        {
            if (singularClient != null)
                return singularClient;

            lock (Sync)
            {
                // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
                singularClient = singularClient ?? Create(log);
                return singularClient;
            }
        }

        private static IClusterClient Create(ILog log = null)
        {
            return new Core.ClusterClient(log ?? LogProvider.Get(),
                configuration =>
                {
                    configuration.AddRequestTransform(request => request.WithHeader(SingularHeaders.XSingularInternalRequest, string.Empty));
                    
                    configuration.SetupUniversalTransport();

                    configuration.SetupWeighedReplicaOrdering(
                        builder =>
                        {
                            var datacenters = DatacentersProvider.Get();
                            builder.AddRelativeWeightModifier(new RelativeWeightSettings());
                            builder.SetupAvoidInactiveDatacentersWeightModifier(datacenters);
                            builder.SetupBoostLocalDatacentersWeightModifier(datacenters);
                        });

                    configuration.TargetEnvironment = SingularConstants.DefaultZone;
                    configuration.TargetServiceName = SingularConstants.ServiceName;

                    configuration.SetupClusterConfigTopology(ClusterConfigClient.Default, SingularConstants.CCTopologyName);
                    
                    configuration.DefaultRequestStrategy = Strategy.Forking(SingularClientConstants.ForkingStrategyParallelismLevel);
                    
                    configuration.MaxReplicasUsedPerRequest = SingularClientConstants.ForkingStrategyParallelismLevel;
                });
        }
    }
}