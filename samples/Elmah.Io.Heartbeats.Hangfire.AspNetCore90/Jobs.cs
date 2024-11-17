namespace Elmah.Io.Heartbeats.Hangfire.AspNetCore90
{
    public static class Jobs
    {
        [ElmahIoHeartbeat("API_KEY", "LOG_ID", "HEARTBEAT_ID")]
        public static void Test()
        {
            Console.WriteLine("{0} Recurring job completed successfully!", DateTime.Now.ToString());
        }
    }
}
