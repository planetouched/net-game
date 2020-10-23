using System;
using Server.Simulations;
using Shared.Loggers;

namespace DedicatedServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLogger(new ConsoleLogger());

            var server = new ServerSimulation(12345, 1);
            server.StartSimulation();

            while (true)
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Q)
                    break;

                if (key.Key == ConsoleKey.G)
                {
                    Logger.Log("GC Collect");
                    GC.Collect();
                }
            }

            server.StopSimulation();
        }
    }
}
