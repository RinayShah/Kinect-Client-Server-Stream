using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleLib.Server;
namespace ExampleConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            var server = new Server(2, new List<int>(new []{10, 15}));
            server.Start();
            Console.Read();
        }
    }
}
