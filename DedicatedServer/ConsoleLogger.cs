using System;
using Shared.Loggers;

namespace DedicatedServer
{
    class ConsoleLogger : ILogger
    {
        public void Log(string str)
        {
            Console.WriteLine(str);
        }

        public void LogWarning(string str)
        {
            Console.WriteLine("Warning:" + str);
        }

        public void LogError(string str)
        {
            Console.WriteLine("Error: " + str);
        }
    }
}
