
using AkkaSb.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class SbAkkaTest
    {

        private AkkaSbConfig getLocaSysConfig()
        {
            AkkaSbConfig cfg = new AkkaSbConfig();
            cfg.SbConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";
            cfg.LocalNode = new NodeConfig() { ReceiveQueue = "actorsystem/rcvlocal", SendQueue = "actorsystem/sendlocal" };
            cfg.RemoteNodes = new Dictionary<string, NodeConfig>();
            cfg.RemoteNodes.Add("actorsystem/remote1", new NodeConfig() { ReceiveQueue = "actorsystem/rcvnode1", SendQueue = "actorsystem/sendnode1" });
            cfg.RemoteNodes.Add("actorsystem/remote2", new NodeConfig() { ReceiveQueue = "actorsystem/rcvnode2", SendQueue = "actorsystem/sendnode2" });

            return cfg;
        }

        private AkkaSbConfig getRemote1SysConfig()
        {
            var localCfg = getLocaSysConfig();

            AkkaSbConfig cfg = new AkkaSbConfig();
            cfg.SbConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";
            cfg.LocalNode = new NodeConfig() { ReceiveQueue = localCfg.RemoteNodes.First().Value.SendQueue, SendQueue = localCfg.RemoteNodes.First().Value.ReceiveQueue };

            return cfg;
        }


        static ConcurrentDictionary<object, object> receivedMessages = new ConcurrentDictionary<object, object>();


        public class TestClass
        {
            public int Prop1 { get; set; }

            public string Prop2 { get; set; }
        }

        public class MyActor : ActorBase
        {
            public MyActor(ActorId id) : base(id)
            {
                Receive<string>((str) =>
                {
                    receivedMessages.TryAdd(str, str);
                });

                Receive<TestClass>((c) =>
                {
                    receivedMessages.TryAdd(c, c.ToString());
                });
            }

        }

        [TestMethod]

        public void InitActorSystem()
        {
            var cfg = getLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem(cfg);
            ActorSystem sysRemote = new ActorSystem(getRemote1SysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                await sysRemote.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1, cfg.RemoteNodes.FirstOrDefault().Key);
            actorRef1.Tell("message 1").Wait();

            actorRef1.Tell(new TestClass()).Wait();

            ActorReference actorRef2 = sysLocal.CreateActor<MyActor>(2, cfg.RemoteNodes.FirstOrDefault().Key);
            actorRef2.Tell("message 2").Wait();

            while (true)
            {
                if (receivedMessages.Count == 3)
                {
                    Assert.IsTrue(receivedMessages.Values.Contains("message 1"));
                    Assert.IsTrue(receivedMessages.Values.Contains("message 2"));
                    Assert.IsTrue(receivedMessages.Values.Contains("UnitTestsProject.SbAkkaTest+TestClass"));
                    src.Cancel();
                    break;
                }
                Thread.Sleep(250);
            }

            task.Wait();
        }
    }
}
