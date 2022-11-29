﻿namespace Elmah.Io.Heartbeats.Hangfire.AspNetCore60
{
    public static class Jobs
    {
        [ElmahIoHeartbeat("API_KEY", "LOG_ID", "HEARTBEAT_ID")]
        public static void Test()
        {
            Console.WriteLine("{0} Recurring job completed successfully!", DateTime.Now.ToString());

            // Throw an exception to test Unhealthy heartbeat
            //throw new Exception("Error during job");
        }
    }
}
