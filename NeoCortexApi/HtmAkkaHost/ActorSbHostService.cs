using Akka.Actor;
using Akka.Configuration;
using AkkaSb.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkkaHostLib
{
    public class ActorSbHostService
    {
        ILogger logger;

        private AkkaSb.Net.ActorSystem akkaClusterSystem;

        private CancellationTokenSource tokenSrc = new CancellationTokenSource();

        public ActorSbHostService(ILogger logger = null)
        {
            this.logger = logger;
        }

        public void Start(string[] args)
        {
            AkkaSbConfig cfg = new AkkaSbConfig();

            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            builder.AddEnvironmentVariables();
            IConfigurationRoot configArgs = builder.Build();

            cfg.SbConnStr = configArgs["SbConnStr"];
            cfg.ReplyMsgQueue = configArgs["ReplyMsgQueue"];
            cfg.RequestMsgQueue = configArgs["RequestMsgQueue"];
            string systemName = configArgs["SystemName"];
            
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                tokenSrc.Cancel();
            };

            akkaClusterSystem = new AkkaSb.Net.ActorSystem($"{systemName}", cfg, logger);
            akkaClusterSystem.Start(tokenSrc.Token);

            Console.WriteLine("Press any key to stop Actor SB system.");
            
        }

    }
}
