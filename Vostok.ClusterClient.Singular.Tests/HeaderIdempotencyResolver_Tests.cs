using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Singular.Idempotency;
using Vostok.Singular.Core.PathPatterns.Extensions;
using Vostok.Singular.Core.PathPatterns.Idempotency;

namespace Vostok.ClusterClient.Singular.Tests
{
    public class HeaderIdempotencyResolver_Tests
    {
        [Test]
        public void IsIdempotent_should_detect_idempotency_header()
        {
            var request = Request.Get("test").WithIdempotentHeader();

            var idempotent = HeaderIdempotencyResolver.IsIdempotentByHeader(request.GetIdempotencyHeader());

            idempotent.Should().BeTrue();
        }

        [Test]
        public void IsIdempotent_should_detect_non_idempotency_header()
        {
            var request = Request.Get("test").WithNotIdempotentHeader();

            var idempotent = HeaderIdempotencyResolver.IsIdempotentByHeader(request.GetIdempotencyHeader());

            idempotent.Should().BeFalse();
        }

        [Test]
        public void IsIdempotent_should_return_null_when_no_header()
        {
            var request = Request.Get("test");

            var idempotent = HeaderIdempotencyResolver.IsIdempotentByHeader(request.GetIdempotencyHeader());

            idempotent.Should().BeNull();
        }
    }
}