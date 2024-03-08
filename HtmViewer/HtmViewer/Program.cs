using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HtmViewer
{
    public class Program
    {
        public static void Main(string[] args)
        {


            //CreateWebHostBuilder(args).Build().Run();
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                //.UseUrls($"http://localhost:{config.GetValue<int>("Host:Port")}")
                .ConfigureKestrel((context, options) =>
                {
                    // Set properties and call methods on options
                    context.Configuration.GetSection("Kestrel");
                })
                .ConfigureLogging((hostingcontext, logging) =>
                {
                    logging.AddConfiguration(hostingcontext.Configuration.GetSection("Logging"));
                    logging.AddConsole();

                })

                .Build();
            host.Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
