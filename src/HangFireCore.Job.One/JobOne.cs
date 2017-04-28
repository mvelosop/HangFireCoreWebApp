using HangFireCore.Core;
using NLog;
using System;
using System.Threading.Tasks;

namespace HangFireCore.Job.One
{
    [HangfireJobMinutes(1)]
    public class JobOne
    {
        // Get logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static DateTime start = DateTime.Now;

        public static Task Execute()
        {
            // Use logger at INFO level
            return Task.Run(() => logger.Info(@"Executing Job One - {0:hh\:mm\:ss\.fff}", DateTime.Now - start));
        }
    }
}
