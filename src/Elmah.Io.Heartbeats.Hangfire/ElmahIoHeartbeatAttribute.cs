﻿using Elmah.Io.Client;
using Hangfire.Common;
using Hangfire.Server;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Elmah.Io.Heartbeats.Hangfire
{
    /// <summary>
    /// Decorate jobs with this filter to automatically log heartbeats to elmah.io.
    /// </summary>
    public class ElmahIoHeartbeatAttribute : JobFilterAttribute, IServerFilter
    {
        internal static string _assemblyVersion = typeof(ElmahIoHeartbeatAttribute).Assembly.GetName().Version.ToString();
        internal static string _hangfireAssemblyVersion = typeof(JobFilterAttribute).Assembly.GetName().Version.ToString();

        private const string StopwatchKeyName = "elmahio-timing";
        private readonly Guid logId;
        private readonly string heartbeatId;
        private readonly IHeartbeatsClient heartbeats;

        /// <summary>
        /// Creates a new instance of the attribute. Provide the API key, log ID and heartbeat ID found in the elmah.io UI.
        /// </summary>
        public ElmahIoHeartbeatAttribute(string apiKey, string logId, string heartbeatId)
        {
            var elmahioApi = ElmahioAPI.Create(apiKey);
            elmahioApi.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io.Heartbeats.Hangfire", _assemblyVersion)));
            elmahioApi.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Hangfire.Core", _hangfireAssemblyVersion)));
            heartbeats = elmahioApi.Heartbeats;
            this.logId = new Guid(logId);
            this.heartbeatId = heartbeatId;
        }

        /// <summary>
        /// Called by Hangire just before executing the job.
        /// </summary>
        public void OnPerforming(PerformingContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.Items.Add(StopwatchKeyName, stopwatch);
        }

        /// <summary>
        /// Called by Hangfire just after executing the job.
        /// </summary>
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
