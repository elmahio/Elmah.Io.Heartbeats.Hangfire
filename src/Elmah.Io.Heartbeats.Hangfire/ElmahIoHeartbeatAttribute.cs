using Elmah.Io.Client;
using Hangfire.Common;
using Hangfire.Server;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Elmah.Io.Heartbeats.Hangfire
{
    /// <summary>
    /// Decorate jobs with this filter to automatically log heartbeats to elmah.io.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class ElmahIoHeartbeatAttribute : JobFilterAttribute, IServerFilter
    {
        private static readonly string _assemblyVersion = typeof(ElmahIoHeartbeatAttribute).Assembly.GetName().Version.ToString();
        private static readonly string _hangfireAssemblyVersion = typeof(JobFilterAttribute).Assembly.GetName().Version.ToString();

        private const string StopwatchKeyName = "elmahio-timing";
        private readonly Guid logId;
        private readonly string heartbeatId;
        private readonly IHeartbeatsClient heartbeats;

        private ElmahIoHeartbeatAttribute(Guid logId, string heartbeatId)
        {
            this.logId = logId;
            this.heartbeatId = heartbeatId;
        }

        internal ElmahIoHeartbeatAttribute(IHeartbeatsClient heartbeatsClient, string logId, string heartbeatId) : this(new Guid(logId), heartbeatId)
        {
            heartbeats = heartbeatsClient;
        }

        /// <summary>
        /// Creates a new instance of the attribute. Provide the API key, log ID and heartbeat ID found in the elmah.io UI.
        /// </summary>
        public ElmahIoHeartbeatAttribute(string apiKey, string logId, string heartbeatId) : this(new Guid(logId), heartbeatId)
        {
            var elmahioApi = ElmahioAPI.Create(apiKey, new ElmahIoOptions
            {
                UserAgent = UserAgent(),
            });
            heartbeats = elmahioApi.Heartbeats;
        }

        /// <summary>
        /// Called by Hangire just before executing the job.
        /// </summary>
        public void OnPerforming(PerformingContext filterContext)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            filterContext.Items.Add(StopwatchKeyName, stopwatch);
        }

        /// <summary>
        /// Called by Hangfire just after executing the job.
        /// </summary>
        public void OnPerformed(PerformedContext filterContext)
        {
            long? took = null;
            if (filterContext.Items.ContainsKey(StopwatchKeyName) && filterContext.Items[StopwatchKeyName] is Stopwatch stopwatch)
            {
                stopwatch.Stop();
                took = stopwatch.ElapsedMilliseconds;
            }

            if (filterContext.Exception != null)
            {
                heartbeats.Unhealthy(logId, heartbeatId, filterContext.Exception.ToString(), took: took);
            }
            else
            {
                heartbeats.Healthy(logId, heartbeatId, took: took);
            }
        }

        private static string UserAgent()
        {
            return new StringBuilder()
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io.Heartbeats.Hangfire", _assemblyVersion)).ToString())
                .Append(" ")
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Hangfire.Core", _hangfireAssemblyVersion)).ToString())
                .ToString();
        }
    }
}
