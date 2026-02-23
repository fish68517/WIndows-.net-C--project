using System;
using System.Configuration;
using System.IO;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// Initializes and manages the logging system for the application.
    /// Handles log file creation, rotation, and configuration.
    /// </summary>
    public class LogInitializer
    {
        private static readonly string LogFilePath = ConfigurationManager.AppSettings["LogFilePath"] ?? "Logs";
        private static readonly string LogLevel = ConfigurationManager.AppSettings["LogLevel"] ?? "Debug";
        private static readonly int LogMaxFileSizeMB = int.Parse(ConfigurationManager.AppSettings["LogMaxFileSizeMB"] ?? "10");
        private static readonly string DebugLogFileName = ConfigurationManager.AppSettings["DebugLogFileName"] ?? "debug.log";
        private static readonly string ErrorLogFileName = ConfigurationManager.AppSettings["ErrorLogFileName"] ?? "error.log";
        private static readonly string EmotionAnalysisLogFileName = ConfigurationManager.AppSettings["EmotionAnalysisLogFileName"] ?? "emotion_analysis.log";

        /// <summary>
        /// Initializes the logging system by creating necessary directories and log files.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Create Logs directory if it doesn't exist
                if (!Directory.Exists(LogFilePath))
                {
                    Directory.CreateDirectory(LogFilePath);
                }

                // Create log files if they don't exist
                CreateLogFileIfNotExists(Path.Combine(LogFilePath, DebugLogFileName));
                CreateLogFileIfNotExists(Path.Combine(LogFilePath, ErrorLogFileName));
                CreateLogFileIfNotExists(Path.Combine(LogFilePath, EmotionAnalysisLogFileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing logging system: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a log file if it doesn't already exist.
        /// </summary>
        /// <param name="filePath">The path to the log file.</param>
        private static void CreateLogFileIfNotExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
        }

        /// <summary>
        /// Gets the full path to the debug log file.
        /// </summary>
        /// <returns>The full path to the debug log file.</returns>
        public static string GetDebugLogPath()
        {
            return Path.Combine(LogFilePath, DebugLogFileName);
        }

        /// <summary>
        /// Gets the full path to the error log file.
        /// </summary>
        /// <returns>The full path to the error log file.</returns>
        public static string GetErrorLogPath()
        {
            return Path.Combine(LogFilePath, ErrorLogFileName);
        }

        /// <summary>
        /// Gets the full path to the emotion analysis log file.
        /// </summary>
        /// <returns>The full path to the emotion analysis log file.</returns>
        public static string GetEmotionAnalysisLogPath()
        {
            return Path.Combine(LogFilePath, EmotionAnalysisLogFileName);
        }

        /// <summary>
        /// Gets the configured log level.
        /// </summary>
        /// <returns>The log level as a string.</returns>
        public static string GetLogLevel()
        {
            return LogLevel;
        }

        /// <summary>
        /// Gets the maximum log file size in megabytes.
        /// </summary>
        /// <returns>The maximum log file size in MB.</returns>
        public static int GetMaxLogFileSizeMB()
        {
            return LogMaxFileSizeMB;
        }

        /// <summary>
        /// Gets the log file directory path.
        /// </summary>
        /// <returns>The log file directory path.</returns>
        public static string GetLogDirectory()
        {
            return LogFilePath;
        }

        /// <summary>
        /// Logs a debug message to the debug log file.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        public static void LogDebug(string message)
        {
            try
            {
                string logPath = GetDebugLogPath();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
                RotateLogFileIfNeeded(logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to debug log: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an error message with exception details to the error log file.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception object containing error details.</param>
        public static void LogError(string message, Exception exception)
        {
            try
            {
                string logPath = GetErrorLogPath();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}Exception: {exception.Message}{Environment.NewLine}StackTrace: {exception.StackTrace}";
                File.AppendAllText(logPath, logEntry + Environment.NewLine + new string('-', 80) + Environment.NewLine);
                RotateLogFileIfNeeded(logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to error log: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs emotion analysis results to the emotion analysis log file.
        /// </summary>
        /// <param name="diaryId">The ID of the diary being analyzed.</param>
        /// <param name="emotionIndex">The calculated emotion index (0-100).</param>
        /// <param name="analysisTime">The time taken to perform the analysis in milliseconds.</param>
        public static void LogEmotionAnalysis(int diaryId, int emotionIndex, long analysisTime)
        {
            try
            {
                string logPath = GetEmotionAnalysisLogPath();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DiaryId: {diaryId}, EmotionIndex: {emotionIndex}, AnalysisTime: {analysisTime}ms";
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
                RotateLogFileIfNeeded(logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to emotion analysis log: {ex.Message}");
            }
        }

        /// <summary>
        /// Rotates the log file if it exceeds the maximum size.
        /// </summary>
        /// <param name="logPath">The path to the log file.</param>
        private static void RotateLogFileIfNeeded(string logPath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(logPath);
                long maxSizeBytes = LogMaxFileSizeMB * 1024 * 1024;

                if (fileInfo.Length > maxSizeBytes)
                {
                    string directory = Path.GetDirectoryName(logPath);
                    string fileName = Path.GetFileNameWithoutExtension(logPath);
                    string extension = Path.GetExtension(logPath);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string archivedPath = Path.Combine(directory, $"{fileName}_{timestamp}{extension}");

                    File.Move(logPath, archivedPath);
                    File.Create(logPath).Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rotating log file: {ex.Message}");
            }
        }
    }
}
