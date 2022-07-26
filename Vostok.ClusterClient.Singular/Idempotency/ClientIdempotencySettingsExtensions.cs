using JetBrains.Annotations;

namespace Vostok.Clusterclient.Singular
{
    [PublicAPI]
    public static class ClientIdempotencySettingsExtensions
    {
        /// <summary>
        /// Adds given values to configuration's <see cref="ClientIdempotencySettings.Rules"/> list.
        /// </summary>
        public static ClientIdempotencySettings WithRule(this ClientIdempotencySettings settings, string method,
                                                         string pattern, bool idempotent = true)
        {
            return settings.WithRule(new ClientIdempotencyRule(method, pattern, idempotent));
        }
        
        /// <summary>
        /// Adds given <paramref name="rule"/> to configuration's <see cref="ClientIdempotencySettings.Rules"/> list.
        /// </summary>
        public static ClientIdempotencySettings WithRule(this ClientIdempotencySettings settings, ClientIdempotencyRule rule)
        {
            settings.Rules.Add(rule);
            return settings;
        }
    }
}