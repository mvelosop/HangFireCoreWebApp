using Hangfire;
using HangFireCore.Core;
using Microsoft.AspNetCore.Hosting;
using NLog;
using System;
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

                string location = Assembly.GetEntryAssembly().Location;

                string directory = Path.GetDirectoryName(location);

                // Find modules that follow the job convention
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

                        var assemblyJobs = assembly
                            .ExportedTypes
                            .Where(et => et.GetTypeInfo().GetCustomAttribute<HangfireJobMinutesAttribute>() != null);

                        if (!assemblyJobs.Any())
                        {
                            logger.Info("Didn't find any job.");
                        }

                        foreach (Type job in assemblyJobs)
                        {
                            int minutes = job.GetTypeInfo().GetCustomAttribute<HangfireJobMinutesAttribute>().Minutes;

                            logger.Trace(@"Scheduling recurring job ""{0}"" with {1} minutes interval", job.Name, minutes);

                            MethodInfo executeMethod = job.GetMethod("Execute");

                            if (executeMethod != null)
                            {
                                // Get lambda expression to call the "Execute" method
                                Expression<Action> expression = Expression.Lambda<Action>(Expression.Call(executeMethod));

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
            catch (Exception)
            {

                throw;
            }
        }
    }
}
