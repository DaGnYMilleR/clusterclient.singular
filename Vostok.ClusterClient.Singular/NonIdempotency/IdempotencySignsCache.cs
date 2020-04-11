using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Singular.NonIdempotency.Identifier;
using Vostok.Clusterclient.Singular.NonIdempotency.Settings;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencySignsCache : IIdempotencySignsCache
    {
        private CachingTransform<NonIdempotencySignsSettings, List<NonIdempotencySign>> cache;

        public IdempotencySignsCache(IIdempotencySignsProvider idempotencySignsProvider)
        {
            cache = new CachingTransform<NonIdempotencySignsSettings, List<NonIdempotencySign>>(PreprocessSigns,
                idempotencySignsProvider.Get);
        }

        public List<NonIdempotencySign> Get()
        {
            return cache.Get();
        }

        private static List<NonIdempotencySign> PreprocessSigns(NonIdempotencySignsSettings nonIdempotencySignsSettings)
        {
            var signs = nonIdempotencySignsSettings.Signs;
            var processedSigns = new List<NonIdempotencySign>(signs.Count);

            processedSigns.AddRange(
                signs.Select(
                    sign => new NonIdempotencySign
                    {
                        Method = sign.Method,
                        PathPattern = sign.PathPattern == null ? null : new Wildcard(sign.PathPattern)
                    }));

            return processedSigns;
        }
    }
}