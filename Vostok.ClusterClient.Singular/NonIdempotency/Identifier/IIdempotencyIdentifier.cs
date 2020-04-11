namespace Vostok.Clusterclient.Singular.NonIdempotency.Identifier
{
    internal interface IIdempotencyIdentifier
    {
        bool IsIdempotent(string method, string path);
    }
}