using System;
using System.Data.SQLite;
using System.IO;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// Manages database connection and initialization for the application.
    /// Handles SQLite database creation, schema setup, and connection management.
    /// </summary>
    public static class DatabaseManager
    {
        private static SQLiteConnection _connection;

        /// <summary>
        /// Initializes the database by creating it if it doesn't exist and setting up the schema.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                string databasePath = ConfigurationHelper.DatabasePath;

                // Create database file if it doesn't exist
                if (!File.Exists(databasePath))
                {
                    LogInitializer.LogDebug($"Database file not found at {databasePath}. Creating new database.");
                    SQLiteConnection.CreateFile(databasePath);
                }

                // Open connection and create tables
                using (SQLiteConnection connection = new SQLiteConnection(ConfigurationHelper.DatabaseConnectionString))
                {
                    connection.Open();
                    LogInitializer.LogDebug("Database connection established successfully.");

                    // Create tables if they don't exist
                    CreateTables(connection);

                    connection.Close();
                }

                LogInitializer.LogDebug("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                LogInitializer.LogError("Database initialization failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates a database connection.
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
            {
                _connection = new SQLiteConnection(ConfigurationHelper.DatabaseConnectionString);
                _connection.Open();
            }
            return _connection;
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public static void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// Creates the database schema with Users, Diaries, and Logs tables.
        /// </summary>
        private static void CreateTables(SQLiteConnection connection)
        {
            // Create Users table
            string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLoginAt DATETIME
                );";

            // Create Diaries table
            string createDiariesTable = @"
                CREATE TABLE IF NOT EXISTS Diaries (
                    DiaryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Content TEXT NOT NULL,
                    EmotionIndex INTEGER NOT NULL,
                    IsHighRisk BOOLEAN DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(UserId)
                );";

            // Create Logs table
            string createLogsTable = @"
                CREATE TABLE IF NOT EXISTS Logs (
                    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LogType TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    StackTrace TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(connection, createUsersTable);
            LogInitializer.LogDebug("Users table created or verified.");

            ExecuteNonQuery(connection, createDiariesTable);
            LogInitializer.LogDebug("Diaries table created or verified.");

            ExecuteNonQuery(connection, createLogsTable);
            LogInitializer.LogDebug("Logs table created or verified.");
        }

        /// <summary>
        /// Executes a non-query SQL command.
        /// </summary>
        private static void ExecuteNonQuery(SQLiteConnection connection, string commandText)
        {
            using (SQLiteCommand command = new SQLiteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
