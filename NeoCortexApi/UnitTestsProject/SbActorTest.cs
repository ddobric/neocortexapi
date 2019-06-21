
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
        private const string sbConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";

        private AkkaSbConfig getLocaSysConfig()
        {
            AkkaSbConfig cfg = new AkkaSbConfig();
            cfg.SbConnStr = sbConnStr;
            cfg.LocalNode = new NodeConfig() { ReceiveQueue = "actorsystem/rcvlocal", ReplyQueue = "actorsystem/sendlocal" };
            cfg.RemoteNodes = new Dictionary<string, NodeConfig>();
            cfg.RemoteNodes.Add("actorsystem/remote1", new NodeConfig() { ReceiveQueue = "actorsystem/rcvnode1", ReplyQueue = "actorsystem/sendnode1" });
            cfg.RemoteNodes.Add("actorsystem/remote2", new NodeConfig() { ReceiveQueue = "actorsystem/rcvnode2", ReplyQueue = "actorsystem/sendnode2" });

            return cfg;
        }

        private AkkaSbConfig getRemote1SysConfig()
        {
            var localCfg = getLocaSysConfig();

            AkkaSbConfig cfg = new AkkaSbConfig();
            cfg.SbConnStr = sbConnStr;
            cfg.LocalNode = new NodeConfig() { ReceiveQueue = localCfg.RemoteNodes["actorsystem/remote1"].ReplyQueue, ReplyQueue = localCfg.RemoteNodes["actorsystem/remote1"].ReceiveQueue };

            return cfg;
        }


        private AkkaSbConfig getRemot2SysConfig()
        {
            var localCfg = getLocaSysConfig();

            AkkaSbConfig cfg = new AkkaSbConfig();
            cfg.SbConnStr = sbConnStr;
            cfg.LocalNode = new NodeConfig() { ReceiveQueue = localCfg.RemoteNodes["actorsystem/remote2"].ReplyQueue, ReplyQueue = localCfg.RemoteNodes["actorsystem/remote2"].ReceiveQueue };

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
            public MyActor(ActorId id):base(id)
            {
                Receive<string>((str) =>
                {
                    receivedMessages.TryAdd(str, str);
                    return null;
                });

                Receive<TestClass>((c) =>
                {
                    receivedMessages.TryAdd(c, c.ToString());
                    return null;
                });

                Receive<long>((long num) =>
                {
                    receivedMessages.TryAdd(num, num);
                    return num+1;
                });

                Receive<DateTime>((DateTime dt) =>
                {
                    receivedMessages.TryAdd(dt, dt);
                    return dt.AddDays(1);
                });
            }
        }


        /// <summary>
        /// Tests if Tell() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]

        public void TellTest()
        {
            var cfg = getLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem(cfg);
            ActorSystem sysRemote = new ActorSystem(getRemote1SysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);
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

        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]
        public void AskTest()
        {
            var cfg = getLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem(cfg);
            ActorSystem sysRemote = new ActorSystem(getRemote1SysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1, cfg.RemoteNodes.FirstOrDefault().Key);
        
            var response = actorRef1.Ask<long>((long)42).Result;

            Assert.IsTrue(response == 43);

            response = actorRef1.Ask<long>((long)7).Result;

            Assert.IsTrue(response == 8);
        }


        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]
        public void AskManyNodesTest()
        {
            var cfg = getLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem(cfg);
            ActorSystem sysRemote1 = new ActorSystem(getRemote1SysConfig());
            ActorSystem sysRemote2 = new ActorSystem(getRemote1SysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task1 = Task.Run(() =>
            {
                sysRemote1.Start(src.Token);
            });

            var task2 = Task.Run(() =>
            {
                sysRemote2.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1, cfg.RemoteNodes.FirstOrDefault().Key);
            var response = actorRef1.Ask<long>((long)42).Result;
            Assert.IsTrue(response == 43);

            response = actorRef1.Ask<long>((long)7).Result;
            Assert.IsTrue(response == 8);


            ActorReference actorRef2 = sysLocal.CreateActor<MyActor>(1, cfg.RemoteNodes.LastOrDefault().Key);
            var response2 = actorRef2.Ask<long>((long)10).Result;
            Assert.IsTrue(response2 == 11);

            DateTime dtRes = actorRef2.Ask<DateTime>(new DateTime(2019, 1, 1)).Result;

            Assert.IsTrue(dtRes.Day == 2);
            Assert.IsTrue(dtRes.Year == 2019);
            Assert.IsTrue(dtRes.Month == 1);
        }


        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]
        public void AskManyNodesManyMessagesTest()
        {
            var cfg = getLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem(cfg);
            ActorSystem sysRemote1 = new ActorSystem(getRemote1SysConfig());
            ActorSystem sysRemote2 = new ActorSystem(getRemote1SysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task1 = Task.Run(() =>
            {
                sysRemote1.Start(src.Token);
            });

            var task2 = Task.Run(() =>
            {
                sysRemote2.Start(src.Token);
            });


            Parallel.For(0, 3, (i) =>
            {
                ActorReference actorRef = sysLocal.CreateActor<MyActor>(i, cfg.RemoteNodes.FirstOrDefault().Key);

                for (int k = 0; k < 10; i++)
                {
                    var response = actorRef.Ask<long>((long)k).Result;
                    Assert.IsTrue(response == k + 1);

                    DateTime dtRes = actorRef.Ask<DateTime>(new DateTime(2019, 1, 1)).Result;

                    Assert.IsTrue(dtRes.Day == 2);
                    Assert.IsTrue(dtRes.Year == 2019);
                    Assert.IsTrue(dtRes.Month == 1);
                }
            });
        }
    }
}
