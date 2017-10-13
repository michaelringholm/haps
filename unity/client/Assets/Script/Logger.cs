using System;
using UnityEngine;

namespace Assets.Script
{
    public static class AppLog
    {
        // Send to AWS when possible
        public static void LogLine(string format, params object[] args)
        {
            string line = string.Format(GetTimestamp() + "> " + format, args);
            Debug.LogFormat(line);
        }

        // ISO 8061 timestamp
        private static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
