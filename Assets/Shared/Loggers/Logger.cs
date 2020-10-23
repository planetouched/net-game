namespace Shared.Loggers
{
    public static class Logger
    {
        private static ILogger _logger;
        
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void Log(string str)
        {
            _logger.Log(str);
        }

        public static void LogWarning(string str)
        {
            _logger.LogWarning(str);
        }

        public static void LogError(string str)
        {
            _logger.LogError(str);
        }
    }
}