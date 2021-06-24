using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Singular.Helpers
{
    [PublicAPI]
    public static class RequestParametersExtensions
    {
        internal const string RequestParametersTagsFilterKey = "ReplicasTagsFilter";

        /// <summary>
        /// <para> Sets given <paramref name="filterString" /> replicas filtering expression in string format to <paramref name="requestParameters" />. </para>
        /// <para> This expression will be sent via request headers and executed in Singular. </para> 
        /// <para> If an expression derived from a <paramref name="filterString" /> returns false then replica will be filtered.</para>
        /// </summary>
        public static RequestParameters SetTagsFilter(this RequestParameters requestParameters, string filterString)
            => requestParameters.WithProperty(RequestParametersTagsFilterKey, filterString);

        internal static string GetTagsFilter(this RequestParameters requestParameters) =>
            requestParameters.Properties.TryGetValue(RequestParametersTagsFilterKey, out var filterObject)
                ? filterObject as string
                : null;
    }
}