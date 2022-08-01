using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Singular.Core;

namespace Vostok.Clusterclient.Singular.Helpers
{
    [PublicAPI]
    public static class RequestExtensions
    {
        /// <summary>
        /// Adds idempotency sign to the request
        /// </summary>
        /// <returns></returns>
        public static Request WithIdempotencyHeader(this Request request, bool isIdempotent) =>
            request.WithHeader(SingularHeaders.Idempotent, isIdempotent);
    }
}