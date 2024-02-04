using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiSample
{
    public class MyTraceListener : TraceListener
    {
        // called (in debug-mode) when Debug.Write() is called
        public override void Write(string message)
        {
            // handle/output "message" properly
        }
        // called (in debug-mode) when Debug.WriteLine() is called
        public override void WriteLine(string message)
        {
            Console.WriteLine("------------------");
            Console.WriteLine(message);
            Console.WriteLine("------------------");
        }
    }
}
