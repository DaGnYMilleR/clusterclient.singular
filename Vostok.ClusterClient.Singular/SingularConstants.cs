using JetBrains.Annotations;

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public static class SingularConstants
    {
        /// <summary>
        /// Name of the HTTP header used to denote target environment for requests.
        /// </summary>
        public const string EnvironmentHeader = "X-Singular-Zone";

        /// <summary>
        /// Name of the HTTP header used to denote target service for requests.
        /// </summary>
        public const string ServiceHeader = "X-Singular-Service";

        /// <summary>
        /// Default path to ClusterConfig-based topology containing addresses of Singular replicas.
        /// </summary>
        public const string ClusterConfigTopologyPath = "topology/singular";

        internal const string SingularServiceName = "Singular";

        internal const string ServicesConfigurationNamePrefix = "singular/global/services/";

        internal const int ForkingStrategyParallelismLevel = 3;

        internal static string GetNonIdempotencySigns(string service)
        {
            return $"{ServicesConfigurationNamePrefix}{service}.json";
        }
    }
}
