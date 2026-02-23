using System;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{
    /// <summary>
    /// Data Access Object for User operations.
    /// Handles database operations for storing and retrieving user records.
    /// </summary>
    public class UserDAO
    {
        /// <summary>
        /// Inserts a new user record into the database.
        /// </summary>
        /// <param name="user">The user object to insert.</param>
        /// <returns>True if the insertion was successful, false otherwise.</returns>
        public bool InsertUser(User user)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        INSERT INTO Users (Username, PasswordHash, CreatedAt, LastLoginAt)
                        VALUES (@Username, @PasswordHash, @CreatedAt, @LastLoginAt);";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                        command.Parameters.AddWithValue("@LastLoginAt", user.LastLoginAt ?? (object)DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting user record: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a user record by username from the database.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The user record if found, null otherwise.</returns>
        public User SelectUserByUsername(string username)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT UserId, Username, PasswordHash, CreatedAt, LastLoginAt
                        FROM Users
                        WHERE Username = @Username;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Username = reader["Username"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user by username: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user record by user ID from the database.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>The user record if found, null otherwise.</returns>
        public User SelectUserById(int userId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT UserId, Username, PasswordHash, CreatedAt, LastLoginAt
                        FROM Users
                        WHERE UserId = @UserId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Username = reader["Username"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user by ID: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Updates the last login timestamp for a user.
        /// </summary>
        /// <param name="userId">The user ID to update.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public bool UpdateLastLogin(int userId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        UPDATE Users
                        SET LastLoginAt = @LastLoginAt
                        WHERE UserId = @UserId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UserId", userId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last login: {ex.Message}");
                return false;
            }
        }
    }
}
