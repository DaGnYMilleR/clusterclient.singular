using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Singular;
using Vostok.Context;
using Vostok.Singular.Core;

// ReSharper disable InconsistentNaming

namespace Vostok.ClusterClient.Singular.Tests
{
    [TestFixture]
    public class SingularClientSettings_should
    {
        private const string ServiceName = "SomeService";

        [SetUp]
        public void TestSetup()
        {
            FlowingContext.Properties.Set("forced.sd.environment", null);
        }

        [Test]
        public void return_default_target_environment()
        {
            var settings = new SingularClientSettings(ServiceName);
            settings.TargetEnvironment.Should().Be(SingularConstants.DefaultZone);
        }

        [Test]
        public void return_explicitly_set_target_environment()
        {
            var settings = new SingularClientSettings("CustomEnvironment", ServiceName);
            settings.TargetEnvironment.Should().Be("CustomEnvironment");
        }

        [Test]
        public void return_explicitly_set_target_environment_even_when_forced_sd_environment_is_set()
        {
            var settings = new SingularClientSettings("CustomEnvironment", ServiceName);

            FlowingContext.Properties.Set("forced.sd.environment", "env1");

            settings.TargetEnvironment.Should().Be("CustomEnvironment");
        }

        [Test]
        public void use_forced_sd_environment_as_target_environment()
        {
            var settings = new SingularClientSettings(ServiceName);

            FlowingContext.Properties.Set("forced.sd.environment", "env1");
            settings.TargetEnvironment.Should().Be("env1");

            FlowingContext.Properties.Set("forced.sd.environment", "env2");
            settings.TargetEnvironment.Should().Be("env2");
        }

    }
}