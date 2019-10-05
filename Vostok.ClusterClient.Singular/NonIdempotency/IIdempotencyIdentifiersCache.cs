namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal interface IIdempotencyIdentifiersCache
    {
        NonIdempotencySign[] GetNonIdempotencySigns();
    }
}