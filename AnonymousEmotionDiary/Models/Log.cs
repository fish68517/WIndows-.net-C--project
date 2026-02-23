using System;

namespace AnonymousEmotionDiary.Models
{
    /// <summary>
    /// Represents a log record in the system.
    /// Contains information about debug logs, error logs, and emotion analysis logs.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Gets or sets the unique identifier for the log record.
        /// </summary>
        public int LogId { get; set; }

        /// <summary>
        /// Gets or sets the type of log (e.g., "Debug", "Error", "EmotionAnalysis").
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Gets or sets the log message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace for error logs (optional).
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the log was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the Log class.
        /// </summary>
        public Log()
        {
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the Log class with specified parameters.
        /// </summary>
        /// <param name="logType">The type of log.</param>
        /// <param name="message">The log message.</param>
        /// <param name="stackTrace">The stack trace (optional).</param>
        public Log(string logType, string message, string stackTrace = null)
        {
            LogType = logType;
            Message = message;
            StackTrace = stackTrace;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the log record.
        /// </summary>
        /// <returns>A formatted string containing log information.</returns>
        public override string ToString()
        {
            return $"[{CreatedAt:yyyy-MM-dd HH:mm:ss}] {LogType}: {Message}";
        }
    }
}
