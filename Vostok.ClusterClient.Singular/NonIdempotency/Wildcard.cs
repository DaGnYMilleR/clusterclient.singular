using System.Text.RegularExpressions;

namespace Vostok.Clusterclient.Singular.NonIdempotency
{
    internal class Wildcard : Regex
    {
        public Wildcard(string pattern)
            : base(ConvertPattern(pattern), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
        }

        private static string ConvertPattern(string pattern)
        {
            pattern = Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".");
            return "^" + pattern + "$";
        }
    }
}