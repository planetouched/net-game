using UnityEngine;
using ILogger = Shared.Loggers.ILogger;

namespace Client.Loggers
{
    public class UnityLogger : ILogger
    {
        public void Log(string str)
        {
            Debug.Log(str);
        }

        public void LogWarning(string str)
        {
            Debug.LogWarning(str);
        }

        public void LogError(string str)
        {
            Debug.LogError(str);
        }
    }
}