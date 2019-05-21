using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.ClusterConfig.Client.Abstractions;

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public class SingularClientSettings
    {
        /// <param name="environmentName">See <see cref="TargetEnvironment"/>.</param>
        /// <param name="serviceName">See <see cref="TargetService"/>.</param>
        public SingularClientSettings([NotNull] string environmentName, [NotNull] string serviceName)
        {
            TargetEnvironment = environmentName ?? throw new ArgumentNullException(nameof(environmentName));
            TargetService = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        /// <summary>
        /// Target environment to send requests to. When in doubt, use <c>'default'</c> as a value for this parameter.
        /// </summary>
        [NotNull]
        public string TargetEnvironment { get; }

        /// <summary>
        /// Target service to send requests to. This value must exactly match the name that service uses to register in service discovery system.
        /// </summary>
        [NotNull]
        public string TargetService { get; }

        /// <summary>
        /// Path to ClusterConfig-based topology containing addresses of Singular replicas.
        /// </summary>
        [CanBeNull]
        public string ClusterConfigTopologyPath { get; set; } = SingularConstants.ClusterConfigTopologyPath;

        /// <summary>
        /// <para>Use this property to provide a custom instance of ClusterConfig client to fetch topology specified by <see cref="ClusterConfigTopologyPath"/>.</para>
        /// <para>If not set, default global client instance is used.</para>
        /// </summary>
        [CanBeNull]
        public IClusterConfigClient ClusterConfigClient { get; set; }

        /// <summary>
        /// Explicitly setting a custom <see cref="IClusterProvider"/> prevents the client from using ClusterConfig-based topology specified by <see cref="ClusterConfigTopologyPath"/>.
        /// </summary>
        [CanBeNull]
        public IClusterProvider AlternativeClusterProvider { get; set; }
    }
}