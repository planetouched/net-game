namespace Shared.Loggers
{
    public static class Log
    {
        private static ILogger _logger;
        
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void Write(string str)
        {
            _logger.Log(str);
        }

        public static void WriteWarning(string str)
        {
            _logger.LogWarning(str);
        }

        public static void WriteError(string str)
        {
            _logger.LogError(str);
        }
    }
}