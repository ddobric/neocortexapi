using AkkaHostLib;
using System;

namespace HtmAkkaHost
{
    
    class Program
    { 
       
        static void Main(string[] args)
        {
            Console.WriteLine("Hello HTM Actor Model Cluster!");

            AkkaHostService svc = new AkkaHostService();
            svc.Start(args);
        }
    }
}
