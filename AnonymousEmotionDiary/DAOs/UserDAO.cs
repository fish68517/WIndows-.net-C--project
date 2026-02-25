using System;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{

    public class UserDAO
    {
      
        public bool InsertUser(User user)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

        public User SelectUserByUsername(string username)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

     
        public User SelectUserById(int userId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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


        public bool UpdateLastLogin(int userId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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
