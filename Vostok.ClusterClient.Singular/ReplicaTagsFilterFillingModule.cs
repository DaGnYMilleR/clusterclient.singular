using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Singular.Helpers;
using Vostok.Singular.Core;

namespace Vostok.Clusterclient.Singular
{
    internal class ReplicaTagsFilterFillingModule : IRequestModule
    {
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var filter = context.Parameters.GetTagsFilter();
            if (filter != null)
                context.Request = context.Request.WithHeader(SingularHeaders.ReplicaTagsFilter, filter);

            return await next(context).ConfigureAwait(false);
        }
    }
}