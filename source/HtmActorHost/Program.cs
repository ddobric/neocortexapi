
using AkkaSb.Net;
using Microsoft.Extensions.Logging;
using NeoCortexApi.DistributedComputeLib;
using System;

namespace HtmAkkaHost
{

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello HTM Actor Model Cluster!");

            //AkkaHostService svc = new AkkaHostService();
            //svc.Start(args);
            //--port  8081  --sysname HtmCluster  --hosstname=localhost --publichostname=localhost

            ILoggerFactory factory = LoggerFactory.Create(logBuilder =>
            {
                logBuilder.AddDebug();
                logBuilder.AddConsole();
            });

            HtmActor a = new HtmActor(null);

            //--SystemName=node1 --RequestMsgQueue=actorsystem/actorqueue --ReplyMsgQueue=actorsystem/rcvnode1 --SbConnStr="Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=" --TblStoragePersistenConnStr="DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net" --ActorSystemName=inst701 --SubscriptionName=node1
            ActorSbHostService svc = new ActorSbHostService(factory.CreateLogger(nameof(ActorSbHostService)));
            svc.Start(args);
        }
    }
}
