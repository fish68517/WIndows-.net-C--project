using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{
    /// <summary>
    /// Data access for administrator follow-up records.
    /// </summary>
    public class AdminContactDAO
    {
        public bool InsertContactRecord(AdminContactRecord record)
        {
            try
            {
                using SQLiteConnection connection = DatabaseManager.CreateConnection();
                const string query = @"
                    INSERT INTO AdminContacts (UserId, AdminUserId, EmotionIndexSnapshot, ContactMethod, ContactNote, CreatedAt)
                    VALUES (@UserId, @AdminUserId, @EmotionIndexSnapshot, @ContactMethod, @ContactNote, @CreatedAt);";

                using SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", record.UserId);
                command.Parameters.AddWithValue("@AdminUserId", record.AdminUserId);
                command.Parameters.AddWithValue("@EmotionIndexSnapshot", record.EmotionIndexSnapshot);
                command.Parameters.AddWithValue("@ContactMethod", record.ContactMethod);
                command.Parameters.AddWithValue("@ContactNote", record.ContactNote);
                command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt);

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting admin contact record: {ex.Message}");
                return false;
            }
        }

        public List<AdminContactRecord> SelectContactsByUserId(int userId)
        {
            List<AdminContactRecord> records = new List<AdminContactRecord>();

            try
            {
                using SQLiteConnection connection = DatabaseManager.CreateConnection();
                const string query = @"
                    SELECT c.ContactId, c.UserId, c.AdminUserId, c.EmotionIndexSnapshot, c.ContactMethod, c.ContactNote, c.CreatedAt,
                           u.Username AS Username, a.Username AS AdminUsername
                    FROM AdminContacts c
                    INNER JOIN Users u ON c.UserId = u.UserId
                    INNER JOIN Users a ON c.AdminUserId = a.UserId
                    WHERE c.UserId = @UserId
                    ORDER BY c.CreatedAt DESC;";

                using SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapRecord(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving contact records by user id: {ex.Message}");
            }

            return records;
        }

        public AdminContactRecord SelectLatestContactByUserId(int userId)
        {
            try
            {
                using SQLiteConnection connection = DatabaseManager.CreateConnection();
                const string query = @"
                    SELECT c.ContactId, c.UserId, c.AdminUserId, c.EmotionIndexSnapshot, c.ContactMethod, c.ContactNote, c.CreatedAt,
                           u.Username AS Username, a.Username AS AdminUsername
                    FROM AdminContacts c
                    INNER JOIN Users u ON c.UserId = u.UserId
                    INNER JOIN Users a ON c.AdminUserId = a.UserId
                    WHERE c.UserId = @UserId
                    ORDER BY c.CreatedAt DESC
                    LIMIT 1;";

                using SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapRecord(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving latest contact record: {ex.Message}");
            }

            return null;
        }

        private static AdminContactRecord MapRecord(SQLiteDataReader reader)
        {
            return new AdminContactRecord
            {
                ContactId = Convert.ToInt32(reader["ContactId"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                AdminUserId = Convert.ToInt32(reader["AdminUserId"]),
                Username = reader["Username"]?.ToString() ?? string.Empty,
                AdminUsername = reader["AdminUsername"]?.ToString() ?? string.Empty,
                EmotionIndexSnapshot = Convert.ToInt32(reader["EmotionIndexSnapshot"]),
                ContactMethod = reader["ContactMethod"]?.ToString() ?? string.Empty,
                ContactNote = reader["ContactNote"]?.ToString() ?? string.Empty,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
