using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Singular.Core;

namespace Vostok.Clusterclient.Singular.Idempotency
{
    [PublicAPI]
    public static class RequestExtensions
    {
        /// <summary>
        /// Marks request as not idempotent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Request WithNotIdempotentHeader(this Request request) =>
            request.WithHeader(SingularHeaders.Idempotent, false);

        /// <summary>
        /// Marks request as idempotent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Request WithIdempotentHeader(this Request request) =>
            request.WithHeader(SingularHeaders.Idempotent, true);
    }
}