using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Singular.Core.PathPatterns;
using Vostok.Singular.Core.PathPatterns.Idempotency.IdempotencyControlRules;

namespace Vostok.Clusterclient.Singular
{
    internal class NonIdempotencyHeaderRequestTransform : IRequestTransform
    {
        private readonly IReadOnlyList<IdempotencyControlRule> rules;

        public NonIdempotencyHeaderRequestTransform(ClientIdempotencySettings settings)
        {
            rules = settings.Rules
                .Select(x => new IdempotencyControlRule
                {
                    Method = x.Method,
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    PathPattern = x.Pattern is null ? null : new Wildcard(x.Pattern.TrimStart('/')),
                    IsIdempotent = x.IsIdempotent
                })
                .ToArray();
        }

        public Request Transform(Request request)
        {
            if (!rules.Any())
                return request;

            var path = request.Url
                .GetRequestPath()
                .TrimStart('/');

            var rule = rules.FirstOrDefault(x => PathPatternRuleMatcher.IsMatch(x, request.Method, path));

            if (rule is null)
                return request;

            return rule.IsIdempotent ? request.WithIdempotentHeader() : request.WithNotIdempotentHeader();
        }
    }
}