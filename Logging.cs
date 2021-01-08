using Serilog;

namespace ThinkVoip
{
    public static class Logging
    {
        public static ILogger Logger;

        static Logging()
        {
            Logger = new LoggerConfiguration()
                .WriteTo.File("SyncLog.log")
                .CreateLogger();
        }
    }
}