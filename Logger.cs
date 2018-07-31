using System;

namespace SongStatus
{
    class Logger
    {
        public static void Log<T>(T input) => Console.WriteLine("[Song Status] " + input.ToString());
    }
}
