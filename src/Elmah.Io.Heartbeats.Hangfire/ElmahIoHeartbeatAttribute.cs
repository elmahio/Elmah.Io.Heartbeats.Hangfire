using Elmah.Io.Client;
using Hangfire.Common;
using Hangfire.Server;
using System;
using System.Diagnostics;

namespace Elmah.Io.Heartbeats.Hangfire
{
    public class ElmahIoHeartbeatAttribute : JobFilterAttribute, IServerFilter
    {
        private const string StopwatchKeyName = "elmahio-timing";
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.Items.Add(StopwatchKeyName, stopwatch);
        }

        public void OnPerformed(PerformedContext context)
        {
            long? took = null;
            if (context.Items.ContainsKey(StopwatchKeyName))
            {
                var stopwatch = context.Items[StopwatchKeyName] as Stopwatch;
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    took = stopwatch.ElapsedMilliseconds;
                }
            }

            if (context.Exception != null)
            {
                heartbeats.Unhealthy(logId, heartbeatId, context.Exception.ToString(), took: took);
            }
            else
            {
                heartbeats.Healthy(logId, heartbeatId, took: took);
            }
        }
    }
}
