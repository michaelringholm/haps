using System;

namespace HapsApi.BL
{
    public static class Logger
    {
        public static void LogLine(string format, params object[] args)
        {
            string info = string.Format($"{DateTime.UtcNow}> ");
            format = info + format;
            Console.WriteLine(format, args);
        }
    }
}
