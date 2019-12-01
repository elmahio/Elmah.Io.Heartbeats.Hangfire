using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Elmah.Io.Heartbeats.Hangfire.ConsoleApp.Startup))]

namespace Elmah.Io.Heartbeats.Hangfire.ConsoleApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseWelcomePage("/");

            GlobalConfiguration.Configuration.UseMemoryStorage();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate(
                () => Test(),
                Cron.Minutely);
        }

        [ElmahIoHeartbeat("API_KEY", "LOG_ID", "HEARTBEAT_ID")]
        public void Test()
        {
            Console.WriteLine("{0} Recurring job completed successfully!", DateTime.Now.ToString());
        }
    }
}
