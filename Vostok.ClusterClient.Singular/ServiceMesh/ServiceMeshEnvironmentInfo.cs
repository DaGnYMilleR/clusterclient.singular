using System;
using Vostok.Commons.Environment;

#nullable enable

namespace Vostok.Clusterclient.Singular.ServiceMesh
{
    internal static class ServiceMeshEnvironmentInfo
    {
        private const string UseLocalSingularVariable = "SERVICE_MESH_USE_LOCAL_SINGULAR";
        private const string LocalSingularHostNameVariable = "SERVICE_MESH_LOCAL_SINGULAR_HOST_NAME";
        private const string LocalSingularHttpPortVariable = "SERVICE_MESH_LOCAL_SINGULAR_PORT_HTTP";

        private static readonly Lazy<bool> LazyUseLocalSingular = new Lazy<bool>(
            () =>
            {
                var environmentVariable = Environment.GetEnvironmentVariable(UseLocalSingularVariable);

                return bool.TryParse(environmentVariable, out var flag) && flag;
            });

        private static readonly Lazy<string> LazyLocalSingularHostName = new Lazy<string>(
            () =>
            {
                var environmentVariable = Environment.GetEnvironmentVariable(LocalSingularHostNameVariable);

                if (!string.IsNullOrWhiteSpace(environmentVariable))
                    return environmentVariable;

                return EnvironmentInfo.FQDN;
            });

        private static readonly Lazy<Uri> LazyLocalSingularUri = new Lazy<Uri>(
            () =>
            {
                var httpPortEnvironmentVariable = Environment.GetEnvironmentVariable(LocalSingularHttpPortVariable);

                if (!int.TryParse(httpPortEnvironmentVariable, out var httpPort))
                    return new Uri($"http://{LazyLocalSingularHostName.Value}");

                return new Uri($"http://{LazyLocalSingularHostName.Value}:{httpPort}");
            });

        public static bool UseLocalSingular => LazyUseLocalSingular.Value;
        public static Uri LocalSingularUri => LazyLocalSingularUri.Value;
    }
}