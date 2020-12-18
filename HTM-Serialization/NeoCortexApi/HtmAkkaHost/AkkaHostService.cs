using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaHostLib
{
    public class AkkaHostService
    {

        protected ActorSystem AkkaClusterSystem;

        public Task WhenTerminated => AkkaClusterSystem.WhenTerminated;


        public AkkaHostService()
        {

        }

        public void Start(string hoconConfig, string pubHostName, string hostName, string systemName, string[] seedHosts, int port)
        {
            var strCfg = File.ReadAllText(hoconConfig);

            strCfg = strCfg.Replace("@PORT", port.ToString());

            bool isFirst = true;

            StringBuilder sb = new StringBuilder();

            foreach (var item in seedHosts)
            {
                if (isFirst == false)
                    sb.Append(", ");

                sb.Append($"\"akka.tcp://{systemName}@{item}\"");
                //example: seed - nodes = ["akka.tcp://ClusterSystem@localhost:8081"]

                isFirst = false;
            }

            strCfg = strCfg.Replace("@SEEDHOSTS", sb.ToString());

            if (String.IsNullOrEmpty(pubHostName))
                pubHostName = "localhost";

            strCfg = strCfg.Replace("@PUBHOSTNAME", pubHostName);

            if (String.IsNullOrEmpty(hostName))
                hostName = "localhost";

            strCfg = strCfg.Replace("@HOSTNAME", hostName);

            var config = ConfigurationFactory.ParseString(strCfg);

            Console.WriteLine(strCfg);

            AkkaClusterSystem = ActorSystem.Create(systemName, config);
        }

        public Task Stop()
        {
            return CoordinatedShutdown.Get(AkkaClusterSystem).Run(reason: CoordinatedShutdown.ClrExitReason.Instance);
        }

        public void Start(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            builder.AddEnvironmentVariables();
            IConfigurationRoot cfg = builder.Build();

            var strSeedHosts = cfg["seedhosts"];
            var seedHosts = strSeedHosts == null ? new string[0] : strSeedHosts.Split(',');

            var strPort = cfg["port"];
            int port = -1;
            int.TryParse(strPort, out port);

            var sysName = cfg["sysName"];
            if (String.IsNullOrEmpty(sysName))
                sysName = "LearningAPISystem";

            var pubHostName = cfg["publichostname"];

            var hostName = cfg["hostname"];

            this.Start("akkahost.hocon", pubHostName, hostName, sysName, seedHosts.Select(h => h.TrimStart(' ').TrimEnd(' ')).ToArray(), port);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                this.Stop();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Press any key to stop AKKA.NET Node");
            this.WhenTerminated.Wait();
        }

    }
}
