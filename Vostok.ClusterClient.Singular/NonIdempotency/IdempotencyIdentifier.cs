using System;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class IdempotencyIdentifier : IIdempotencyIdentifier
    {
        private readonly IIdempotencyIdentifiersCache idempotencyIdentifiersCache;

        public IdempotencyIdentifier(IIdempotencyIdentifiersCache idempotencyIdentifiersCache)
        {
            this.idempotencyIdentifiersCache = idempotencyIdentifiersCache;
        }

        public bool IsIdempotent(string method, string path)
        {
            var signs = idempotencyIdentifiersCache.GetNonIdempotencySigns();

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