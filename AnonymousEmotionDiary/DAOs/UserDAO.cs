using System;
using System.Collections.Generic;
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
                        INSERT INTO Users (Username, PasswordHash, Role, ContactInfo, CreatedAt, LastLoginAt)
                        VALUES (@Username, @PasswordHash, @Role, @ContactInfo, @CreatedAt, @LastLoginAt);";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@Role", string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role);
                        command.Parameters.AddWithValue("@ContactInfo", string.IsNullOrWhiteSpace(user.ContactInfo) ? (object)DBNull.Value : user.ContactInfo);
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
                        SELECT UserId, Username, PasswordHash, Role, ContactInfo, CreatedAt, LastLoginAt
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
                                    Role = reader["Role"]?.ToString() ?? "User",
                                    ContactInfo = reader["ContactInfo"] != DBNull.Value ? reader["ContactInfo"].ToString() : string.Empty,
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
                        SELECT UserId, Username, PasswordHash, Role, ContactInfo, CreatedAt, LastLoginAt
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
                                    Role = reader["Role"]?.ToString() ?? "User",
                                    ContactInfo = reader["ContactInfo"] != DBNull.Value ? reader["ContactInfo"].ToString() : string.Empty,
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

        public List<User> SelectAllRegularUsers()
        {
            List<User> users = new List<User>();

            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
                {
                    string query = @"
                        SELECT UserId, Username, PasswordHash, Role, ContactInfo, CreatedAt, LastLoginAt
                        FROM Users
                        WHERE Role <> 'Admin'
                        ORDER BY CreatedAt DESC;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = reader["Role"]?.ToString() ?? "User",
                                ContactInfo = reader["ContactInfo"] != DBNull.Value ? reader["ContactInfo"].ToString() : string.Empty,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all users: {ex.Message}");
            }

            return users;
        }
    }
}
