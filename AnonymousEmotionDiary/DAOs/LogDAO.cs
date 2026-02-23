using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{
    /// <summary>
    /// Data Access Object for Log operations.
    /// Handles database operations for storing and retrieving log records.
    /// </summary>
    public class LogDAO
    {
        /// <summary>
        /// Inserts a new log record into the database.
        /// </summary>
        /// <param name="log">The log object to insert.</param>
        /// <returns>True if the insertion was successful, false otherwise.</returns>
        public bool InsertLog(Log log)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        INSERT INTO Logs (LogType, Message, StackTrace, CreatedAt)
                        VALUES (@LogType, @Message, @StackTrace, @CreatedAt);";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LogType", log.LogType);
                        command.Parameters.AddWithValue("@Message", log.Message);
                        command.Parameters.AddWithValue("@StackTrace", log.StackTrace ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting log record: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves all log records of a specific type from the database.
        /// </summary>
        /// <param name="logType">The type of logs to retrieve (e.g., "Debug", "Error", "EmotionAnalysis").</param>
        /// <returns>A list of log records matching the specified type.</returns>
        public List<Log> SelectLogsByType(string logType)
        {
            List<Log> logs = new List<Log>();

            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT LogId, LogType, Message, StackTrace, CreatedAt
                        FROM Logs
                        WHERE LogType = @LogType
                        ORDER BY CreatedAt DESC;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LogType", logType);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Log log = new Log
                                {
                                    LogId = Convert.ToInt32(reader["LogId"]),
                                    LogType = reader["LogType"].ToString(),
                                    Message = reader["Message"].ToString(),
                                    StackTrace = reader["StackTrace"] != DBNull.Value ? reader["StackTrace"].ToString() : null,
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                                logs.Add(log);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving logs by type: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Retrieves all log records from the database.
        /// </summary>
        /// <returns>A list of all log records.</returns>
        public List<Log> SelectAllLogs()
        {
            List<Log> logs = new List<Log>();

            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT LogId, LogType, Message, StackTrace, CreatedAt
                        FROM Logs
                        ORDER BY CreatedAt DESC;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Log log = new Log
                                {
                                    LogId = Convert.ToInt32(reader["LogId"]),
                                    LogType = reader["LogType"].ToString(),
                                    Message = reader["Message"].ToString(),
                                    StackTrace = reader["StackTrace"] != DBNull.Value ? reader["StackTrace"].ToString() : null,
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                                logs.Add(log);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Deletes log records older than the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to retain logs for.</param>
        /// <returns>The number of log records deleted.</returns>
        public int DeleteOldLogs(int days)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        DELETE FROM Logs
                        WHERE CreatedAt < datetime('now', '-' || @Days || ' days');";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Days", days);
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting old logs: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Retrieves a specific log record by its ID.
        /// </summary>
        /// <param name="logId">The ID of the log record to retrieve.</param>
        /// <returns>The log record if found, null otherwise.</returns>
        public Log SelectLogById(int logId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT LogId, LogType, Message, StackTrace, CreatedAt
                        FROM Logs
                        WHERE LogId = @LogId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LogId", logId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Log
                                {
                                    LogId = Convert.ToInt32(reader["LogId"]),
                                    LogType = reader["LogType"].ToString(),
                                    Message = reader["Message"].ToString(),
                                    StackTrace = reader["StackTrace"] != DBNull.Value ? reader["StackTrace"].ToString() : null,
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving log by ID: {ex.Message}");
            }

            return null;
        }
    }
}
