using log4net;

namespace Cobalt.Core
{
    public static class Logger
    {
        public static ILog Log { get; private set; }

        public static void Construct()
        {
            Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public static void Destruct()
        {
            LogManager.Shutdown();
        }
    }
}
