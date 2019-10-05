namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal interface IIdempotencyIdentifier
    {
        bool IsIdempotent(string method, string path);
    }
}