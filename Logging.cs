using Serilog;

namespace ThinkVoipTool
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