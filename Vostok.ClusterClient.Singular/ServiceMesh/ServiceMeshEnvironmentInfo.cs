using System;

#nullable enable

namespace Vostok.Clusterclient.Singular.ServiceMesh
{
    internal static class ServiceMeshEnvironmentInfo
    {
        private const string UseLocalSingularVariable = "SERVICE_MESH_USE_LOCAL_SINGULAR";

        private static readonly Lazy<bool> LazyUseLocalSingular = new Lazy<bool>(
            () =>
            {
                var environmentVariable = Environment.GetEnvironmentVariable(UseLocalSingularVariable);

                return bool.TryParse(environmentVariable, out var flag) && flag;
            });

        public static bool UseLocalSingular => LazyUseLocalSingular.Value;
    }
}