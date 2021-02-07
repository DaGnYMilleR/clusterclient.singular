using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;

#nullable enable

namespace Vostok.Clusterclient.Singular.ServiceMesh
{
    internal class SingleReplicaWithConnectionTimeoutRequestStrategy : IRequestStrategy
    {
        public static readonly SingleReplicaWithConnectionTimeoutRequestStrategy Instance = new SingleReplicaWithConnectionTimeoutRequestStrategy();

        public Task SendAsync(
            Request request,
            RequestParameters parameters,
            IRequestSender sender,
            IRequestTimeBudget budget,
            IEnumerable<Uri> replicas,
            int replicasCount,
            CancellationToken cancellationToken)
        {
            return sender.SendToReplicaAsync(
                replicas.Single(),
                request,
                parameters.ConnectionTimeout,
                budget.Remaining,
                cancellationToken);
        }

        public override string ToString() => "SingleReplicaWithConnectionTimeout";
    }
}