using System.Collections.Generic;
using Vostok.Clusterclient.Singular.NonIdempotency.Identifier;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal interface IIdempotencySignsCache
    {
        List<NonIdempotencySign> Get();
    }
}