using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Cobalt.Core
{
    public static class Logger
    {
        public static readonly ILog Log = LogManager.GetLogger("Cobalt");

        static Logger()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%message%newline"
            };
            patternLayout.ActivateOptions();

            var coloredConsoleAppender = new ManagedColoredConsoleAppender();
            coloredConsoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors
            {
                ForeColor = System.ConsoleColor.Red,
                Level = log4net.Core.Level.Error
            });

            coloredConsoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors
            {
                ForeColor = System.ConsoleColor.Magenta,
                Level = log4net.Core.Level.Debug
            });

            coloredConsoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors
            {
                ForeColor = System.ConsoleColor.White,
                Level = log4net.Core.Level.Info
            });

            coloredConsoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors
            {
                ForeColor = System.ConsoleColor.Red,
                BackColor = System.ConsoleColor.White,
                Level = log4net.Core.Level.Fatal
            });

            coloredConsoleAppender.Layout = patternLayout;
            coloredConsoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(coloredConsoleAppender);
            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;

            BasicConfigurator.Configure(hierarchy);
        }

        public static void Destruct()
        {
            LogManager.Shutdown();
        }
    }
}
