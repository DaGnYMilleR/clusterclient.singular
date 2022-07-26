using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Singular
{
    /// <summary>
    /// Contains rules to define idempotency for requests.
    /// </summary>
    [PublicAPI]
    public class ClientIdempotencySettings
    {
        public List<ClientIdempotencyRule> Rules { get; } = new();
    }
}