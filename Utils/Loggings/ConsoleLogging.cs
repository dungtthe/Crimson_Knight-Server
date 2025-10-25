using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Utils.Loggings
{
    public static class ConsoleLogging
    {
        private static readonly object _consoleLock = new object();

        public static void LogInfor(string msg)
        {
            if (!ServerSetting.IsConsoleLoggingInfo)
            {
                return;
            }
            lock (_consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[INFO] {msg}");
                Console.ResetColor();
            }
        }

        public static void LogError(string msg)
        {
            if (!ServerSetting.IsConsoleLoggingError)
            {
                return;
            }
            lock (_consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {msg}");
                Console.ResetColor();
            }
        }
        public static void LogWarning(string msg)
        {
            if (!ServerSetting.IsConsoleLoggingWarning)
            {
                return;
            }
            lock (_consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] {msg}");
                Console.ResetColor();
            }
        }
    }
}
