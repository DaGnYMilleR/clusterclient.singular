using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Singular
{
    /// <summary>
    /// Contains rules to define idempotency for requests.
    /// </summary>
    [PublicAPI]
    public class ClientIdempotencySettings
    {
        /// <summary>
        /// Marks all requests as not idempotent
        /// </summary>
        public static ClientIdempotencySettings AllRequestsAreNotIdempotent = new ClientIdempotencySettings().WithRule("*", "*", false);
        
        /// <summary>
        /// Marks all requests as idempotent
        /// </summary>
        public static ClientIdempotencySettings AllRequestsAreIdempotent = new ClientIdempotencySettings().WithRule("*", "*");
        
        public List<ClientIdempotencyRule> Rules { get; } = new();
    }
}