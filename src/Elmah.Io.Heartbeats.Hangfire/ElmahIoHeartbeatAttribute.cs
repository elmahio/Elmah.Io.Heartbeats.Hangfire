using Elmah.Io.Client;
using Hangfire.Common;
using Hangfire.Server;
using System;

namespace Elmah.Io.Heartbeats.Hangfire
{
    public class ElmahIoHeartbeatAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly Guid logId;
        private readonly string heartbeatId;
        private readonly IHeartbeats heartbeats;

        public ElmahIoHeartbeatAttribute(string apiKey, string logId, string heartbeatId)
        {
            heartbeats = ElmahioAPI.Create(apiKey).Heartbeats;
            this.logId = new Guid(logId);
            this.heartbeatId = heartbeatId;
        }

        public void OnPerforming(PerformingContext context)
        {
        }

        public void OnPerformed(PerformedContext context)
        {
            if (context.Exception != null)
            {
                heartbeats.Unhealthy(logId, heartbeatId, context.Exception.ToString());
            }
            else
            {
                heartbeats.Healthy(logId, heartbeatId);
            }
        }
    }
}
