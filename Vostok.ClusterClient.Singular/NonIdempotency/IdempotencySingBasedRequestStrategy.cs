using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Strategies.DelayProviders;
using Vostok.ClusterConfig.Client.Abstractions;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    public class IdempotencySingBasedRequestStrategy : IRequestStrategy
    {
        private const int ForkingStrategyParallelismLevel = 3;
        private readonly IRequestStrategy sequentialStrategy;
        private readonly IRequestStrategy forkingStrategy;
        private IIdempotencyIdentifier idempotencyIdentifier;

        public IdempotencySingBasedRequestStrategy(IClusterConfigClient clusterConfigClient, string service)
        {
            sequentialStrategy = Strategy.Sequential1;
            forkingStrategy = new ForkingRequestStrategy(new EqualDelaysProvider(ForkingStrategyParallelismLevel), ForkingStrategyParallelismLevel);
            var idempotencyIdentifiersCache = new IdempotencyIdentifiersCache(clusterConfigClient, service);
            idempotencyIdentifier = new IdempotencyIdentifier(idempotencyIdentifiersCache);
        }

        public Task SendAsync(
            Request request, 
            RequestParameters parameters, 
            IRequestSender sender, 
            IRequestTimeBudget budget, 
            IEnumerable<Uri> replicas, 
            int replicasCount, 
            CancellationToken cancellationToken)
        {
            var selectedStrategy = idempotencyIdentifier.IsIdempotent(request.Method, request.Url.AbsolutePath) ? forkingStrategy : sequentialStrategy;

            return selectedStrategy.SendAsync(request, parameters, sender, budget, replicas, replicasCount, cancellationToken);
        }
    }
}