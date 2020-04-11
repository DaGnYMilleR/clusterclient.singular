using System;

namespace Vostok.Clusterclient.Singular.NonIdempotency.Identifier
{
    internal class IdempotencyIdentifier : IIdempotencyIdentifier
    {
        private readonly IIdempotencySignsCache idempotencySignsCache;

        public IdempotencyIdentifier(IIdempotencySignsCache idempotencySignsCache)
        {
            this.idempotencySignsCache = idempotencySignsCache;
        }

        public bool IsIdempotent(string method, string path)
        {
            var signs = idempotencySignsCache.Get();

            foreach (var sign in signs)
            {
                if (sign.Method == null || sign.PathPattern == null)
                    continue;

                if (!string.Equals(sign.Method, method, StringComparison.OrdinalIgnoreCase) || path == null)
                    continue;
                
                if (sign.PathPattern.IsMatch(path))
                    return false;
            }

            return true;
        }
    }
}