
using AkkaSb.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    [TestClass]
    public class SbAkkaTest
    {
        private const string sbConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";

        private const string tblAccountConnStr = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";

        internal static ActorSbConfig GetLocaSysConfig()
        {
            ActorSbConfig cfg = new ActorSbConfig();
            cfg.SbConnStr = sbConnStr;
            cfg.ReplyMsgQueue = "actorsystem/rcvlocal";
            cfg.RequestMsgTopic = "actorsystem/actortopic";
            cfg.TblStoragePersistenConnStr = tblAccountConnStr;
            cfg.ActorSystemName = "inst701";
            return cfg;
        }
        
        internal static ActorSbConfig GetRemoteSysConfig(string node = "default")
        {
            var localCfg = GetLocaSysConfig();

            ActorSbConfig cfg = new ActorSbConfig();
            cfg.SbConnStr = sbConnStr;
            cfg.RequestMsgTopic = "actorsystem/actortopic";
            cfg.RequestSubscriptionName = node;
            cfg.ReplyMsgQueue = null;
             
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

                Receive<TestClass>(((c) =>
                {
                    receivedMessages.TryAdd(c, c.ToString());
                    return null;
                }));

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
            Debug.WriteLine($"Start of {nameof(TellTest)}");

            var cfg = GetLocaSysConfig();
            ActorSystem sysLocal = new AkkaSb.Net.ActorSystem($"{nameof(TellTest)}/local", cfg);
            ActorSystem sysRemote = new ActorSystem($"{nameof(TellTest)}/remote", GetRemoteSysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1);
            actorRef1.Tell("message 1").Wait();

            actorRef1.Tell(new TestClass()).Wait();

            ActorReference actorRef2 = sysLocal.CreateActor<MyActor>(2);
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

            Debug.WriteLine($"End of {nameof(TellTest)}");
        }

        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]
        public void AskTest()
        {
            Debug.WriteLine($"Start of {nameof(AskTest)}");

            ActorSystem sysRemote = new ActorSystem($"{nameof(AskTest)}/remote", GetRemoteSysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                sysRemote.Start(src.Token);
            });

            var cfg = GetLocaSysConfig();

            ActorSystem sysLocal = new ActorSystem($"{nameof(AskTest)}/local", cfg);

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1);
        
            var response = actorRef1.Ask<long>((long)42).Result;

            Assert.IsTrue(response == 43);

            response = actorRef1.Ask<long>((long)7).Result;

            Assert.IsTrue(response == 8);

            Debug.WriteLine($"End of {nameof(AskTest)}");
        }

        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        //[TestMethod]
        //[TestCategory("SbActorTests")]
        //[TestCategory("SbActorHostRequired")]
        public void AskTestClientOnly()
        {
            //Thread.Sleep(2000);
            Debug.WriteLine($"Start of {nameof(AskTest)}");

            var cfg = GetLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem($"{nameof(AskTest)}/local", cfg);
           
            CancellationTokenSource src = new CancellationTokenSource();

            ActorReference actorRef1 = sysLocal.CreateActor<HtmActor>(1);

            var response = actorRef1.Ask<string>(new PingNodeMsg() { Msg = ":)" }).Result;

            Assert.IsTrue(response == $"Ping back - :)");
           
            Debug.WriteLine($"End of {nameof(AskTest)}");
        }


        /// <summary>
        /// Tests if Ask() works as designed.
        /// </summary>

        [TestMethod]
        [TestCategory("SbActorTests")]
        public void AskManyNodesTest()
        {
            var cfg = GetLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem("local", cfg);
            ActorSystem sysRemote1 = new ActorSystem("node1", GetRemoteSysConfig());
            ActorSystem sysRemote2 = new ActorSystem("node2", GetRemoteSysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task1 = Task.Run(() =>
            {
                sysRemote1.Start(src.Token);
            });

            var task2 = Task.Run(() =>
            {
                sysRemote2.Start(src.Token);
            });

            ActorReference actorRef1 = sysLocal.CreateActor<MyActor>(1);
            var response = actorRef1.Ask<long>((long)42).Result;
            Assert.IsTrue(response == 43);

            response = actorRef1.Ask<long>((long)7).Result;
            Assert.IsTrue(response == 8);

            ActorReference actorRef2 = sysLocal.CreateActor<MyActor>(7);
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
            var cfg = GetLocaSysConfig();
            ActorSystem sysLocal = new ActorSystem("local", cfg);
            ActorSystem sysRemote1 = new ActorSystem("remote1", GetRemoteSysConfig());
            ActorSystem sysRemote2 = new ActorSystem("remote2", GetRemoteSysConfig());

            CancellationTokenSource src = new CancellationTokenSource();

            var task1 = Task.Run(() =>
            {
                sysRemote1.Start(src.Token);
            });

            var task2 = Task.Run(() =>
            {
                sysRemote2.Start(src.Token);
            });

            //Thread.Sleep(int.MaxValue);

            Parallel.For(0, 20, (i) =>
            {
                ActorReference actorRef = sysLocal.CreateActor<MyActor>(i);

                for (int k = 0; k < 5; k++)
                {
                    var response = actorRef.Ask<long>((long)k).Result;
                    Assert.IsTrue(response == k + 1);

                    DateTime dtRes = actorRef.Ask<DateTime>(new DateTime(2019, 1, 1 + i % 17)).Result;

                    Assert.IsTrue(dtRes.Day == 2 + i % 17);
                    Assert.IsTrue(dtRes.Year == 2019);
                    Assert.IsTrue(dtRes.Month == 1);
                }
            });
        }
    }
}
