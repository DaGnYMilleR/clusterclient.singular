using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Singular.Core.PathPatterns.Idempotency;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencySingBasedRequestStrategy : IRequestStrategy
    {
        private readonly IRequestStrategy sequential1Strategy;
        private readonly IRequestStrategy forkingStrategy;
        private readonly IIdempotencyIdentifier idempotencyIdentifier;

        public IdempotencySingBasedRequestStrategy(IIdempotencyIdentifier idempotencyIdentifier, IRequestStrategy sequential1Strategy, IRequestStrategy forkingStrategy)
        {
            this.sequential1Strategy = sequential1Strategy;
            this.forkingStrategy = forkingStrategy;
            this.idempotencyIdentifier = idempotencyIdentifier;
        }

        public async Task SendAsync(
            Request request,
            RequestParameters parameters,
            IRequestSender sender,
            IRequestTimeBudget budget,
            IEnumerable<Uri> replicas,
            int replicasCount,
            CancellationToken cancellationToken)
        {
            var path = GetRequestPath(request.Url);

            var selectedStrategy = await idempotencyIdentifier.IsIdempotent(request.Method, path).ConfigureAwait(false) ? forkingStrategy : sequential1Strategy;

            await selectedStrategy.SendAsync(request, parameters, sender, budget, replicas, replicasCount, cancellationToken).ConfigureAwait(false);
        }

        private static string GetRequestPath(Uri url)
        {
            if (url.IsAbsoluteUri)
                return url.AbsolutePath;

            var originalString = url.OriginalString;
            var queryIndex = originalString.IndexOf('?');
            return queryIndex > -1 ? originalString.Substring(0, queryIndex) : originalString;
        }
    }
}