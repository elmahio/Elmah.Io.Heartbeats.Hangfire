using Elmah.Io.Client;
using NSubstitute;
using NUnit.Framework;
using System;
using HangfireRoot = Hangfire;
using HangfireServer = Hangfire.Server;
using HangfireStorage = Hangfire.Storage;

namespace Elmah.Io.Heartbeats.Hangfire.Tests
{
    public class ElmahIoHeartbeatAttributeTest
    {
        private ElmahIoHeartbeatAttribute sut;
        private IHeartbeatsClient heartbeatsClientMock;
        private Guid logId;
        private string heartbeatId;

        [SetUp]
        public void SetUp()
        {
            logId = Guid.NewGuid();
            heartbeatId = Guid.NewGuid().ToString();
            heartbeatsClientMock = Substitute.For<IHeartbeatsClient>();
            sut = new ElmahIoHeartbeatAttribute(heartbeatsClientMock, logId.ToString(), heartbeatId);
        }

        [Test]
        public void CanReportHealthyOnNoException()
        {
            // Arrange
            var performedContext = PerformedContext(null);

            // Act
            sut.OnPerformed(performedContext);

            // Assert
            heartbeatsClientMock.Received(1).Healthy(logId, heartbeatId, null, null, null, null);
            heartbeatsClientMock.DidNotReceive().Unhealthy(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long?>());
        }

        [Test]
        public void CanReportUnhealthyOnException()
        {
            // Arrange
            var exception = new ApplicationException();
            var performedContext = PerformedContext(exception);

            // Act
            sut.OnPerformed(performedContext);

            // Assert
            heartbeatsClientMock.Received(1).Unhealthy(logId, heartbeatId, exception.ToString(), null, null, null);
            heartbeatsClientMock.DidNotReceive().Healthy(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long?>());
        }

        private static HangfireServer.PerformedContext PerformedContext(Exception exception)
        {
            return new HangfireServer.PerformedContext(
                new HangfireServer.PerformContext(
                    Substitute.For<HangfireStorage.IStorageConnection>(),
                    new HangfireRoot.BackgroundJob(Guid.NewGuid().ToString(), null, DateTime.UtcNow),
                    Substitute.For<HangfireRoot.IJobCancellationToken>()),
                null,
                false,
                exception);
        }
    }
}