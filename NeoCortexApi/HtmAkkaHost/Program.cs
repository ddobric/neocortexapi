using AkkaHostLib;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using Microsoft.Extensions.Logging;

namespace HtmAkkaHost
{

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello HTM Actor Model Cluster!");
            Console.WriteLine("Hello HTM Actor Model Cluster!");
            //AkkaHostService svc = new AkkaHostService();
            //svc.Start(args);
            //--port  8081  --sysname HtmCluster  --hosstname=localhost --publichostname=localhost

            LoggerFactory factory = new LoggerFactory();

            factory.AddConsole(LogLevel.Information);
            factory.AddDebug(LogLevel.Information);

            //--SystemName=node1 --RequestMsgQueue=actorsystem/actorqueue --ReplyMsgQueue=actorsystem/rcvnode1 --SbConnStr="Endpoint=sb://actorsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=VKHsVqYHFqjAScWUrX/zg/6JidYvgN29LmKOnqgQ1vs=" --ActorSystemName=inst701 --SubscriptionName=node1
            ActorSbHostService svc = new ActorSbHostService(factory.CreateLogger("logger"));
            svc.Start(args);
        }
    }
}
