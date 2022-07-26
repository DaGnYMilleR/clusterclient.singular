using JetBrains.Annotations;

namespace Vostok.Clusterclient.Singular
{
    /// <summary>
    /// Represents a rule used to define idempotency of client request.
    /// </summary>
    [PublicAPI]
    public class ClientIdempotencyRule
    {
        /// <param name="method">See <see cref="Method" />.</param>
        /// <param name="pattern">See <see cref="Pattern" />.</param>
        /// <param name="isIdempotent">See <see cref="IsIdempotent" />.</param>
        public ClientIdempotencyRule(string method, string pattern, bool isIdempotent)
        {
            Method = method;
            Pattern = pattern;
            IsIdempotent = isIdempotent;
        }

        /// <summary>
        /// Request method or "*" to take all.
        /// </summary>
        public string Method { get; }
        
        /// <summary>
        /// Pattern for request path.
        /// </summary>
        public string Pattern { get; }
        
        /// <summary>
        /// Defines should request be idempotent or not.
        /// </summary>
        public bool IsIdempotent { get; }
    }
}