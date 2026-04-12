using System;
using System.Data.SQLite;
using System.IO;
using BCrypt.Net;

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
                    EnsureUserColumns(connection);
                    SeedAdminAccount(connection);

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
        /// Note: Do NOT use 'using' statement with this connection as it's managed by DatabaseManager.
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
        /// Creates a new database connection for single use.
        /// Use this with 'using' statement for operations that need a fresh connection.
        /// </summary>
        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(ConfigurationHelper.DatabaseConnectionString);
            connection.Open();
            return connection;
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
                    Role TEXT NOT NULL DEFAULT 'User',
                    ContactInfo TEXT,
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

            string createAdminContactsTable = @"
                CREATE TABLE IF NOT EXISTS AdminContacts (
                    ContactId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    AdminUserId INTEGER NOT NULL,
                    EmotionIndexSnapshot INTEGER NOT NULL,
                    ContactMethod TEXT NOT NULL,
                    ContactNote TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(UserId),
                    FOREIGN KEY (AdminUserId) REFERENCES Users(UserId)
                );";

            ExecuteNonQuery(connection, createUsersTable);
            LogInitializer.LogDebug("Users table created or verified.");

            ExecuteNonQuery(connection, createDiariesTable);
            LogInitializer.LogDebug("Diaries table created or verified.");

            ExecuteNonQuery(connection, createLogsTable);
            LogInitializer.LogDebug("Logs table created or verified.");

            ExecuteNonQuery(connection, createAdminContactsTable);
            LogInitializer.LogDebug("AdminContacts table created or verified.");
        }

        private static void EnsureUserColumns(SQLiteConnection connection)
        {
            EnsureColumnExists(connection, "Users", "Role", "TEXT NOT NULL DEFAULT 'User'");
            EnsureColumnExists(connection, "Users", "ContactInfo", "TEXT");
        }

        private static void EnsureColumnExists(SQLiteConnection connection, string tableName, string columnName, string columnDefinition)
        {
            string pragmaQuery = $"PRAGMA table_info({tableName});";
            using SQLiteCommand command = new SQLiteCommand(pragmaQuery, connection);
            using SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string existingColumnName = reader["name"]?.ToString() ?? string.Empty;
                if (string.Equals(existingColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ExecuteNonQuery(connection, $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};");
            LogInitializer.LogDebug($"Column {columnName} added to {tableName}.");
        }

        private static void SeedAdminAccount(SQLiteConnection connection)
        {
            string username = ConfigurationHelper.AdminDefaultUsername;
            string password = ConfigurationHelper.AdminDefaultPassword;

            string selectQuery = "SELECT UserId, Role FROM Users WHERE Username = @Username LIMIT 1;";
            using SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection);
            selectCommand.Parameters.AddWithValue("@Username", username);
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            if (reader.Read())
            {
                string currentRole = reader["Role"]?.ToString() ?? "User";
                if (!string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    reader.Close();
                    using SQLiteCommand updateCommand = new SQLiteCommand("UPDATE Users SET Role = 'Admin' WHERE Username = @Username;", connection);
                    updateCommand.Parameters.AddWithValue("@Username", username);
                    updateCommand.ExecuteNonQuery();
                    LogInitializer.LogDebug($"Existing account '{username}' upgraded to administrator.");
                }
                return;
            }

            reader.Close();

            string insertQuery = @"
                INSERT INTO Users (Username, PasswordHash, Role, ContactInfo, CreatedAt)
                VALUES (@Username, @PasswordHash, 'Admin', '校方管理员值班电话 / 邮箱', @CreatedAt);";

            using SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@Username", username);
            insertCommand.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(password));
            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            insertCommand.ExecuteNonQuery();

            LogInitializer.LogDebug($"Default administrator account '{username}' seeded.");
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
