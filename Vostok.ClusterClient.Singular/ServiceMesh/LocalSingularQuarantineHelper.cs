using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Logging.Abstractions;

#nullable enable

namespace Vostok.Clusterclient.Singular.ServiceMesh
{
    // todo (andrew, 27.01.2021): metrics in fallback and cooldown mechanics
    internal static class LocalSingularQuarantineHelper
    {
        private const int SpinLockIterations = 10;

        private const double AliveHealth = 1.0;
        private const double DeadHealth = 0.05;
        private const double DieRate = 0.86; // 20 consecutive failures to fall from AliveHealth to DeadHealth
        private const double AliveRate = 1.35; // 10 consecutive successes to climb from DeadHealth to AliveHealth

        private static double localSingularHealth = AliveHealth;

        public static bool ShouldUseLocalSingularForNextRequest()
        {
            if (!ServiceMeshEnvironmentInfo.UseLocalSingular)
                return false;

            var currentHealth = ReadHealth();

            return ThreadSafeRandom.NextDouble() < currentHealth;
        }

        public static void HandleLocalSingularFailure(ILog log)
        {
            var currentHealth = ReadHealth();

            for (var i = 0; i < SpinLockIterations; i++)
            {
                var newHealth = currentHealth * DieRate;
                if (newHealth < DeadHealth)
                    newHealth = DeadHealth;

                if (TryUpdateHealth(ref currentHealth, newHealth))
                    return;
            }

            log.Warn($"Failed to update localSingularHealth due to high contention. CurrentHealth: {currentHealth}");
        }

        public static void HandleLocalSingularSuccess(ILog log)
        {
            var currentHealth = ReadHealth();

            for (var i = 0; i < SpinLockIterations; i++)
            {
                var newHealth = currentHealth * AliveRate;
                if (newHealth > AliveHealth)
                    newHealth = AliveHealth;

                if (TryUpdateHealth(ref currentHealth, newHealth))
                    return;
            }

            log.Warn($"Failed to update localSingularHealth due to high contention. CurrentHealth: {currentHealth}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ReadHealth()
        {
            return Interlocked.CompareExchange(ref localSingularHealth, value: 0, comparand: 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static bool TryUpdateHealth(ref double currentHealth, double newHealth)
        {
            var prevHealth = currentHealth;

            currentHealth = Interlocked.CompareExchange(ref localSingularHealth, value: newHealth, comparand: currentHealth);

            return currentHealth == prevHealth;
        }
    }
}