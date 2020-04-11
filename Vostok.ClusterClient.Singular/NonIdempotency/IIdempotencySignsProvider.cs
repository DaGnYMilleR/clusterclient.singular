using Vostok.Clusterclient.Singular.NonIdempotency.Settings;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal interface IIdempotencySignsProvider
    {
        NonIdempotencySignsSettings Get();
    }
}