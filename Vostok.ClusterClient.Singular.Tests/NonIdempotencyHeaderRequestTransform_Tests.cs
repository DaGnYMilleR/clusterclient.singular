using System.Linq;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Singular;

namespace Vostok.ClusterClient.Singular.Tests
{
    public class NonIdempotencyHeaderRequestTransform_Tests
    {
        [Test]
        public void Transform_should_do_nothing_when_no_rules()
        {
            var transformer = CreateTransformerWithRule(null, null);
            var request = Request.Get("path");

            var transformedRequest = transformer.Transform(request);

            transformedRequest.GetIdempotencyHeader().Should().BeNull();
        }

        [Test]
        public void Transform_should_set_header_to_suitable_for_rules_request()
        {
            var transformer = CreateTransformerWithRule(RequestMethods.Get, "some/path");
            var request = Request.Get("some/path");

            var transformedRequest = transformer.Transform(request);

            transformedRequest.GetIdempotencyHeader().Should().NotBeNull();
        }

        [Test]
        public void Transform_should_NOT_set_header_to_unsuitable_for_rules_request()
        {
            var transformer = CreateTransformerWithRule(RequestMethods.Get, "some/path");
            var request = Request.Post("not/exists");

            var transformedRequest = transformer.Transform(request);

            transformedRequest.GetIdempotencyHeader().Should().BeNull();
        }
        
        private static NonIdempotencyHeaderRequestTransform CreateTransformerWithRule(string method, string path)
        {
            if (method is null || path is null)
                return new NonIdempotencyHeaderRequestTransform(new ClientIdempotencySettings());
            return new NonIdempotencyHeaderRequestTransform(new ClientIdempotencySettings().WithRule(method, path, false));
        }
    }
}