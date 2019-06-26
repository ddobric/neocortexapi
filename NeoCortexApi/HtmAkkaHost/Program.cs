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

            AkkaHostService svc = new AkkaHostService();
            svc.Start(args);
            //--port  8081  --sysname HtmCluster  --hostname=localhost --publichostname=localhost

            LoggerFactory factory = new LoggerFactory();
          
            factory.AddConsole(LogLevel.Information);
            factory.AddDebug(LogLevel.Information);

            //--SystemName=node1 RequestMsgQueue=actorsystem/actorqueue ReplyMsgQueue=actorsystem/rcvnode1 --SbConnStr="Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ="
            //ActorSbHostService svc = new ActorSbHostService(factory.CreateLogger("logger"));
            //svc.Start(args);
        }
    }
}
