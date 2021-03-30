using System;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Logging.Abstractions;
using Vostok.Singular.Core;
using Vostok.Singular.Core.PathPatterns.Idempotency;

#nullable enable

namespace Vostok.Clusterclient.Singular.ServiceMesh
{
    internal class ServiceMeshRequestModule : IRequestModule
    {
        private readonly ILog log;
        private readonly IIdempotencyIdentifier idempotencyIdentifier;

        public ServiceMeshRequestModule(ILog log, IIdempotencyIdentifier idempotencyIdentifier)
        {
            this.log = log;
            this.idempotencyIdentifier = idempotencyIdentifier;
        }

        public async Task<ClusterResult> ExecuteAsync(IRequestContext requestContext, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var useLocalSingular = LocalSingularQuarantineHelper.ShouldUseLocalSingularForNextRequest();

            if (useLocalSingular)
            {
                var requestContextTuner = new RequestContextTuner(requestContext);

                requestContextTuner.PrepareForLocalSingular(requestContext);

                var localSingularResult = await next(requestContext).ConfigureAwait(false);

                var localSingularResponse = localSingularResult.ReplicaResults.SingleOrDefault()?.Response;
                if (localSingularResponse == null)
                    return localSingularResult;

                if (LocalSingularFailed(localSingularResponse))
                    LocalSingularQuarantineHelper.HandleLocalSingularFailure(log);
                else
                    LocalSingularQuarantineHelper.HandleLocalSingularSuccess(log);

                if (MustReturnLocalSingularResult(requestContext, localSingularResponse))
                    return localSingularResult;

                var canFallbackOnGlobalSingular = LocalSingularFailedAndRequestCanBeSafelyRetried(localSingularResponse) ||
                                                  await RequestIsIdempotentAsync(requestContext).ConfigureAwait(false);
                if (!canFallbackOnGlobalSingular)
                    return localSingularResult;

                requestContext.CancellationToken.ThrowIfCancellationRequested();

                requestContextTuner.PrepareToFallback(requestContext);
            }

            return await next(requestContext).ConfigureAwait(false);
        }

        private static bool MustReturnLocalSingularResult(IRequestContext requestContext, Response localSingularResponse)
        {
            if (requestContext.Budget.HasExpired)
                return true;

            if (requestContext.Request.ContainsAlreadyUsedStream())
                return true;

            if (localSingularResponse.HasHeaders && localSingularResponse.Headers[HeaderNames.DontRetry] != null)
                return true;

            return false;
        }

        private static bool LocalSingularFailed(Response localSingularResponse)
        {
            return LocalSingularFailedAndRequestCanBeSafelyRetried(localSingularResponse) ||
                   localSingularResponse.Code == ResponseCode.SendFailure ||
                   localSingularResponse.Code == ResponseCode.ReceiveFailure;
        }

        // note (andrew, 29.01.2021):
        // maybe account for local singular failure due to zookeeper connection loss (BadRequest status code produced by Singular.RequestValidator)
        // maybe we need some background health check mechanics which will ping local singular and enable fallback on global singular unconditionally
        private static bool LocalSingularFailedAndRequestCanBeSafelyRetried(Response localSingularResponse)
        {
            return localSingularResponse.Code == ResponseCode.ConnectFailure ||
                   IsThrottledBySingularItSelf(localSingularResponse);
        }

        private static bool IsThrottledBySingularItSelf(Response singularReplicaResponse)
        {
            return singularReplicaResponse.HasHeaders &&
                   string.Equals(
                       SingularHeaders.ThrottlingTriggerReason.ServerThrottlingQueueOverflow,
                       singularReplicaResponse.Headers[SingularHeaders.SingularThrottlingTrigger],
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<bool> RequestIsIdempotentAsync(IRequestContext context)
        {
            var requestPath = IdempotencySignBasedRequestStrategy.GetRequestPath(context.Request.Url);
            var requestIsIdempotent = await idempotencyIdentifier.IsIdempotentAsync(context.Request.Method, requestPath).ConfigureAwait(false);
            return requestIsIdempotent;
        }

        private class RequestContextTuner
        {
            private static readonly FixedClusterProvider LocalSingularClusterProvider = new FixedClusterProvider(ServiceMeshEnvironmentInfo.LocalSingularUri);
            private static readonly AsIsReplicaOrdering LocalSingularReplicaOrdering = new AsIsReplicaOrdering();

            private readonly IClusterProvider fallbackClusterProvider;
            private readonly IReplicaOrdering fallbackReplicaOrdering;
            private readonly int fallbackConnectionAttempts;
            private readonly IRequestStrategy? fallbackRequestStrategy;

            public RequestContextTuner(IRequestContext requestContext)
            {
                fallbackClusterProvider = requestContext.ClusterProvider;
                fallbackReplicaOrdering = requestContext.ReplicaOrdering;
                fallbackConnectionAttempts = requestContext.ConnectionAttempts;
                fallbackRequestStrategy = requestContext.Parameters.Strategy;
            }

            public void PrepareForLocalSingular(IRequestContext requestContext)
            {
                requestContext.ClusterProvider = LocalSingularClusterProvider;
                requestContext.ReplicaOrdering = LocalSingularReplicaOrdering;
                requestContext.ConnectionAttempts = 1;
                requestContext.Parameters = requestContext.Parameters.WithStrategy(SingleReplicaWithConnectionTimeoutRequestStrategy.Instance);
            }

            public void PrepareToFallback(IRequestContext requestContext)
            {
                requestContext.ResetReplicaResults();

                requestContext.ClusterProvider = fallbackClusterProvider;
                requestContext.ReplicaOrdering = fallbackReplicaOrdering;
                requestContext.ConnectionAttempts = fallbackConnectionAttempts;
                requestContext.Parameters = requestContext.Parameters.WithStrategy(fallbackRequestStrategy);
            }
        }
    }
}