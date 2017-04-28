using Hangfire;
using HangFireCore.Core;
using Microsoft.AspNetCore.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;

namespace HangFireCore.WebApp.Helpers
{
    public static class ScheduleJobsHelpers
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        public static void ScheduleRecurringJobs(this IHostingEnvironment env)
        {
            try
            {
                logger.Info("Scheduling recurring jobs...");
                logger.Trace("Loading job modules...");

                // Get the current executing assembly path
                string location = Assembly.GetEntryAssembly().Location;
                string directory = Path.GetDirectoryName(location);

                // Find modules that follow the job module name convention
                var jobModules = Directory.EnumerateFiles(directory, "HangFireCore.Job.*.dll", SearchOption.TopDirectoryOnly);

                if (!jobModules.Any())
                {
                    logger.Info("Didn't find any job module.");
                }

                foreach (var module in jobModules)
                {
                    try
                    {
                        logger.Info("Loading Job assembly: {0}", module);
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(module);

                        logger.Trace("Getting jobs...");

                        // Get types using the HangfireJobMinutes attribute
                        var recurringJobs = assembly
                            .ExportedTypes
                            .Where(et => et.GetTypeInfo().GetCustomAttribute<HangfireJobMinutesAttribute>() != null);

                        if (!recurringJobs.Any())
                        {
                            logger.Info("Didn't find any recurring job.");
                        }

                        foreach (Type job in recurringJobs)
                        {
                            int minutes = job.GetTypeInfo().GetCustomAttribute<HangfireJobMinutesAttribute>().Minutes;

                            logger.Trace(@"Scheduling recurring job ""{0}"" with {1} minutes interval", job.Name, minutes);

                            // We expect every job to have an "Execute" method
                            MethodInfo executeMethod = job.GetMethod("Execute");

                            if (executeMethod != null)
                            {
                                // Get lambda expression to call the STATIC "Execute" method
                                Expression<Action> expression = Expression.Lambda<Action>(Expression.Call(executeMethod));

                                // Add the job to Hangfire's queue
                                RecurringJob.AddOrUpdate(job.FullName, expression, Cron.MinuteInterval(minutes));
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
