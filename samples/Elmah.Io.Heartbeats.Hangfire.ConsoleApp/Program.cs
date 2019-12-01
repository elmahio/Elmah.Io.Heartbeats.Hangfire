using System;
using System.IO;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Microsoft.Owin.Hosting;

namespace Elmah.Io.Heartbeats.Hangfire.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());

            const string endpoint = "http://localhost:12345";

            using (WebApp.Start<Startup>(endpoint))
            {
                Console.WriteLine();
                Console.WriteLine("{0} Hangfire Server started.", DateTime.Now);
                Console.WriteLine();
                Console.WriteLine("{0} Type JOB to add a background job.", DateTime.Now);
                Console.WriteLine("{0} Press ENTER to exit...", DateTime.Now);

                string command;
                while ((command = Console.ReadLine()) != String.Empty)
                {
                    if ("job".Equals(command, StringComparison.OrdinalIgnoreCase))
                    {
                        BackgroundJob.Enqueue(() => Console.WriteLine("{0} Background job completed successfully!", DateTime.Now.ToString()));
                    }
                }
            }
        }
    }
}
