using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExampleLib;

namespace ExampleConsole
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Client client = new Client();
            Trace.Listeners.Add(new ConsoleTraceListener());

            client.NetData += Client_NetData;
            client.Start(new CancellationToken());

            for (;;)
            {
                client.Write(Console.ReadLine());
            }
        }

        private static void Client_NetData(object sender, Client.NetDataEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
