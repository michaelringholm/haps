using UnityEngine;

namespace Assets.Script
{
    public static class AppLog
    {
        // Send to AWS when possible
        public static void LogLine(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }
    }
}
