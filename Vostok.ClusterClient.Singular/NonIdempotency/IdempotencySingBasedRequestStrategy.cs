﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Singular.NonIdempotency.Identifier;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencySingBasedRequestStrategy : IRequestStrategy
    {
        private readonly IRequestStrategy sequential1Strategy;
        private readonly IRequestStrategy forkingStrategy;
        private IIdempotencyIdentifier idempotencyIdentifier;

        public IdempotencySingBasedRequestStrategy(IIdempotencyIdentifier idempotencyIdentifier, IRequestStrategy sequential1Strategy, IRequestStrategy forkingStrategy)
        {
            this.sequential1Strategy = sequential1Strategy;
            this.forkingStrategy = forkingStrategy;
            this.idempotencyIdentifier = idempotencyIdentifier;
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
            var selectedStrategy = idempotencyIdentifier.IsIdempotent(request.Method, request.Url.AbsolutePath) ? forkingStrategy : sequential1Strategy;

            return selectedStrategy.SendAsync(request, parameters, sender, budget, replicas, replicasCount, cancellationToken);
        }
    }
}