using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Singular.Core.QualityMetrics;

namespace Vostok.Clusterclient.Singular
{
    internal class MetricsModule : IRequestModule
    {
        private MetricsProvider metricsProvider;

        public MetricsModule(MetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider;
        }
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var result = await next(context).ConfigureAwait(false);

            metricsProvider?.RecordRequest(ClusterResultsAnalyzer.FindResultSource(result));

            return result;
        }
    }
}
