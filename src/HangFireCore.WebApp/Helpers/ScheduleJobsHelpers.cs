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
                logger.Trace("Scheduling recurring jobs...");
                logger.Trace("Loading job modules...");

                string location = Assembly.GetEntryAssembly().Location;

                string directory = Path.GetDirectoryName(location);

                var jobModules = Directory.EnumerateFiles(directory, "HangFireCore.Job.*.dll", SearchOption.TopDirectoryOnly);

                foreach (var module in jobModules)
                {
                    try
                    {
                        logger.Trace("Loading Job assembly: {0}", module);
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(module);

                        logger.Trace("Getting jobs...");

                        var assemblyJobs = assembly
                            .ExportedTypes
                            .Where(et => et.GetTypeInfo().GetCustomAttribute<HangfireMinutesJobAttribute>() != null);

                        foreach (Type job in assemblyJobs)
                        {
                            int minutes = job.GetTypeInfo().GetCustomAttribute<HangfireMinutesJobAttribute>().Minutes;

                            logger.Trace("Scheduling Job {0} with {1} minutes interval", job.Name, minutes);

                            MethodInfo executeMethod = job.GetMethod("Execute");

                            Expression<Action> expression = Expression.Lambda<Action>(Expression.Call(executeMethod));

                            RecurringJob.AddOrUpdate(job.FullName, expression, Cron.MinuteInterval(minutes));
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
