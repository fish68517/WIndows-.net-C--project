using System;
using System.IO;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Service for managing application logging operations.
    /// Handles debug logs, error logs, and emotion analysis logs with file rotation support.
    /// </summary>
    public class LogService
    {
        private readonly string _logDirectory;
        private readonly int _maxFileSizeMB;
        private readonly string _debugLogPath;
        private readonly string _errorLogPath;
        private readonly string _emotionAnalysisLogPath;

        /// <summary>
        /// Initializes a new instance of the LogService class.
        /// </summary>
        public LogService()
        {
            _logDirectory = ConfigurationHelper.LogFilePath;
            _maxFileSizeMB = ConfigurationHelper.LogMaxFileSizeMB;
            _debugLogPath = Path.Combine(_logDirectory, ConfigurationHelper.DebugLogFileName);
            _errorLogPath = Path.Combine(_logDirectory, ConfigurationHelper.ErrorLogFileName);
            _emotionAnalysisLogPath = Path.Combine(_logDirectory, ConfigurationHelper.EmotionAnalysisLogFileName);

            EnsureLogDirectory();
        }

        /// <summary>
        /// Ensures the log directory exists.
        /// </summary>
        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Logs a debug message with timestamp to the debug log file.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        public void LogDebug(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                AppendToLogFile(_debugLogPath, logEntry);
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
        public void LogError(string message, Exception exception)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}" +
                                  $"Exception: {exception?.Message}{Environment.NewLine}" +
                                  $"StackTrace: {exception?.StackTrace}";
                AppendToLogFile(_errorLogPath, logEntry + Environment.NewLine + new string('-', 80));
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
        /// <param name="analysisTime">The time when the analysis was performed.</param>
        /// <param name="modelVersion">The model version used for analysis (e.g., "keyword+llm" or "keyword-only").</param>
        public void LogEmotionAnalysis(int diaryId, int emotionIndex, DateTime analysisTime, string modelVersion = "unknown")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DiaryId: {diaryId}, " +
                                  $"EmotionIndex: {emotionIndex}, AnalysisTime: {analysisTime:yyyy-MM-dd HH:mm:ss}, " +
                                  $"ModelVersion: {modelVersion}";
                AppendToLogFile(_emotionAnalysisLogPath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to emotion analysis log: {ex.Message}");
            }
        }

        /// <summary>
        /// Appends a log entry to the specified log file and rotates if necessary.
        /// </summary>
        /// <param name="logPath">The path to the log file.</param>
        /// <param name="logEntry">The log entry to append.</param>
        private void AppendToLogFile(string logPath, string logEntry)
        {
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
            RotateLogFileIfNeeded(logPath);
        }

        /// <summary>
        /// Rotates the log file if it exceeds the maximum size (10MB by default).
        /// Archives the current log file with a timestamp and creates a new one.
        /// </summary>
        /// <param name="logPath">The path to the log file.</param>
        private void RotateLogFileIfNeeded(string logPath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(logPath);
                long maxSizeBytes = _maxFileSizeMB * 1024 * 1024;

                if (fileInfo.Length > maxSizeBytes)
                {
                    string directory = Path.GetDirectoryName(logPath);
                    string fileName = Path.GetFileNameWithoutExtension(logPath);
                    string extension = Path.GetExtension(logPath);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string archivedPath = Path.Combine(directory, $"{fileName}_{timestamp}{extension}");

                    // Move current log file to archived location
                    if (File.Exists(logPath))
                    {
                        File.Move(logPath, archivedPath, true);
                    }

                    // Create new empty log file
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
