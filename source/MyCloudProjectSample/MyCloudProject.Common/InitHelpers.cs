using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Logging.Console;
using System.Threading;

using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MyCloudProject.Common
{
    public static class InitHelpers
    {
        /// <summary>
        /// Create Logging infrastructure in the Trainer Workload
        /// </summary>
        /// <returns></returns>
        public static ILoggerFactory InitLogging(IConfigurationRoot configRoot)
        {
            //create logger from the appsettings addConsole Debug to Logg 
            return LoggerFactory.Create(logBuilder =>
            {
                ConsoleLoggerOptions logCfg = new ConsoleLoggerOptions();

                logBuilder.AddConfiguration(configRoot.GetSection("Logging"));

                logBuilder.AddConsole((opts) =>
                {
                    opts.IncludeScopes = true;
                }).AddDebug();
            });
        }

        /// <summary>
        /// Look into appconfig.json and initialize configurations for the Training Workload
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot InitConfiguration(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("MYCLOUDPROJECT_ENVIRONMENT");

            ConfigurationBuilder builder = new ConfigurationBuilder();
            if (string.IsNullOrEmpty(environmentName))
            {
                builder.AddJsonFile(System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json"), false, true);
            }
            else
            {
                builder.AddJsonFile(System.IO.Path.Combine(AppContext.BaseDirectory, $"appsettings.{environmentName}.json"), false, true);
            }

            if (args != null)
                builder.AddCommandLine(args);
            builder.AddEnvironmentVariables();
            var configRoot = builder.Build();

           
            return configRoot;
        }

    


     
    }
}
