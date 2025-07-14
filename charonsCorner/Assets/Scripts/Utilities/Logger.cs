using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Utilities
{
    public static class Logger
    {
        public enum LogLevel
        {
            INFO,
            WARNING,
            ERROR
        }

        public readonly static Dictionary<LogLevel, bool> LogsEnabled = new Dictionary<LogLevel, bool>
        {
            { LogLevel.INFO, true },
            { LogLevel.WARNING, true },
            { LogLevel.ERROR, true }
        };

        public readonly static Dictionary<LogLevel, string> LogColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.INFO, "white" },
            { LogLevel.WARNING, "yellow" },
            { LogLevel.ERROR, "red" }
        };

        public static void Log(string message)
        {
            if (LogsEnabled[LogLevel.INFO])
                Debug.Log(Format(LogLevel.INFO, message));
        }

        public static void LogWarning(string message)
        {
            if (LogsEnabled[LogLevel.WARNING])
                Debug.LogWarning(Format(LogLevel.WARNING, message));
        }

        public static void LogError(string message)
        {
            if (LogsEnabled[LogLevel.ERROR])
                Debug.LogError(Format(LogLevel.ERROR, message));
        }

        private static string Format(LogLevel level, string message)
        {
            return $"<color={LogColors[level]}>[{Time.time:F2}] [{level}] {message}</color>";
        }
    }
}
