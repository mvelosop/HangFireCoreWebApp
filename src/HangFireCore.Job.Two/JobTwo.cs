﻿using HangFireCore.Core;
using NLog;
using System;
using System.Threading.Tasks;

namespace HangFireCore.Job.One
{
    [HangfireJobMinutes(2)]
    public class JobTwo
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime start = DateTime.Now;

        public static Task Execute()
        {
            return Task.Run(() => logger.Info(@"Executing Job One - {0:hh\:mm\:ss\.fff}", DateTime.Now - start));
        }
    }
}
