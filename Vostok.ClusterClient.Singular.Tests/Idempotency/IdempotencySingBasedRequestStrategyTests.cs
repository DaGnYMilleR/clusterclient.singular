using System;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Singular.NonIdempotency;
using Vostok.Singular.Core.PathPatterns.Idempotency;

namespace Vostok.Clusterclient.Singular.Tests.Idempotency
{
    [TestFixture]
    class IdempotencySingBasedRequestStrategyTests
    {
        private IIdempotencyIdentifier idempotencyIdentifier;
        private IRequestStrategy sequential1Strategy;
        private IRequestStrategy forkingStrategy;
        private IdempotencySingBasedRequestStrategy strategy;
        private Request request;

        [SetUp]
        public void SetUp()
        {
            idempotencyIdentifier = Substitute.For<IIdempotencyIdentifier>();
            sequential1Strategy = Substitute.For<IRequestStrategy>();
            forkingStrategy = Substitute.For<IRequestStrategy>();
            strategy = new IdempotencySingBasedRequestStrategy(idempotencyIdentifier, sequential1Strategy, forkingStrategy);
            request = Request.Get("http://localhost:80/foo/bar");
        }

        [TestCase("http://localhost:80/foo/bar", "/foo/bar")]
        [TestCase("http://localhost:80/foo/bar?orgId=1&userId=qwerty", "/foo/bar")]
        [TestCase("http://localhost:80", "/")]
        [TestCase("http://localhost:80/", "/")]
        public void Should_correct_extract_relative_path_from_absolute_urls(string url, string expectedPath)
        {
            request = Request.Get(url);
            strategy.SendAsync(request, null, null, null, null, 1, CancellationToken.None).Wait();
            idempotencyIdentifier.Received(1).IsIdempotentAsync(request.Method, expectedPath);
        }

        [TestCase("/foo/bar", "/foo/bar")]
        [TestCase("/foo/bar?orgId=1&userId=qwerty", "/foo/bar")]
        [TestCase("/", "/")]
        [TestCase("/?param=True", "/")]
        [TestCase("/?", "/")]
        [TestCase("?", "")]
        public void Should_correct_extract_relative_path_from_relative_urls(string url, string expectedPath)
        {
            request = Request.Get(new Uri(url, UriKind.Relative));
            strategy.SendAsync(request, null, null, null, null, 1, CancellationToken.None).Wait();
            idempotencyIdentifier.Received(1).IsIdempotentAsync(request.Method, expectedPath);
        }

        [Test]
        public void IdempotencySingBasedRequestStrategy_should_select_sequential_strategy_for_non_idempotent_requests()
        {
            idempotencyIdentifier.IsIdempotentAsync("", "").ReturnsForAnyArgs(false);

            strategy.SendAsync(request, null, null, null, null, 1, CancellationToken.None).Wait();
            sequential1Strategy.Received(1).SendAsync(request, null, null, null, null, 1, CancellationToken.None);
            forkingStrategy.DidNotReceive().SendAsync(request, null, null, null, null, 1, CancellationToken.None);
        }

        [Test]
        public void IdempotencySingBasedRequestStrategy_should_select_forking_strategy_for_idempotent_requests()
        {
            idempotencyIdentifier.IsIdempotentAsync("", "").ReturnsForAnyArgs(true);

            strategy.SendAsync(request, null, null, null, null, 1, CancellationToken.None).Wait();
            sequential1Strategy.DidNotReceive().SendAsync(request, null, null, null, null, 1, CancellationToken.None);
            forkingStrategy.Received(1).SendAsync(request, null, null, null, null, 1, CancellationToken.None);
        }
    }
}