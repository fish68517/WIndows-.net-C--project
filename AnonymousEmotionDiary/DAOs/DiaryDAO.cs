using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{
    /// <summary>
    /// Data Access Object for Diary operations.
    /// Handles database operations for storing, retrieving, and managing diary records.
    /// </summary>
    public class DiaryDAO
    {
        /// <summary>
        /// Inserts a new diary record into the database.
        /// </summary>
        /// <param name="diary">The diary object to insert.</param>
        /// <returns>True if the insertion was successful, false otherwise.</returns>
        public bool InsertDiary(Diary diary)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        INSERT INTO Diaries (UserId, Content, EmotionIndex, IsHighRisk, CreatedAt)
                        VALUES (@UserId, @Content, @EmotionIndex, @IsHighRisk, @CreatedAt);";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", diary.UserId);
                        command.Parameters.AddWithValue("@Content", diary.Content);
                        command.Parameters.AddWithValue("@EmotionIndex", diary.EmotionIndex);
                        command.Parameters.AddWithValue("@IsHighRisk", diary.IsHighRisk ? 1 : 0);
                        command.Parameters.AddWithValue("@CreatedAt", diary.CreatedAt);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting diary record: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a diary record by diary ID from the database.
        /// </summary>
        /// <param name="diaryId">The diary ID to search for.</param>
        /// <returns>The diary record if found, null otherwise.</returns>
        public Diary SelectDiaryById(int diaryId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT DiaryId, UserId, Content, EmotionIndex, IsHighRisk, CreatedAt
                        FROM Diaries
                        WHERE DiaryId = @DiaryId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DiaryId", diaryId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Diary
                                {
                                    DiaryId = Convert.ToInt32(reader["DiaryId"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Content = reader["Content"].ToString(),
                                    EmotionIndex = Convert.ToInt32(reader["EmotionIndex"]),
                                    IsHighRisk = Convert.ToBoolean(reader["IsHighRisk"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving diary by ID: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Retrieves all diary records for a specific user from the database.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>A list of diary records for the user, or an empty list if none found.</returns>
        public List<Diary> SelectDiariesByUserId(int userId)
        {
            List<Diary> diaries = new List<Diary>();

            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        SELECT DiaryId, UserId, Content, EmotionIndex, IsHighRisk, CreatedAt
                        FROM Diaries
                        WHERE UserId = @UserId
                        ORDER BY CreatedAt DESC;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                diaries.Add(new Diary
                                {
                                    DiaryId = Convert.ToInt32(reader["DiaryId"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Content = reader["Content"].ToString(),
                                    EmotionIndex = Convert.ToInt32(reader["EmotionIndex"]),
                                    IsHighRisk = Convert.ToBoolean(reader["IsHighRisk"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving diaries by user ID: {ex.Message}");
            }

            return diaries;
        }

        /// <summary>
        /// Deletes a diary record from the database.
        /// </summary>
        /// <param name="diaryId">The diary ID to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public bool DeleteDiary(int diaryId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        DELETE FROM Diaries
                        WHERE DiaryId = @DiaryId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DiaryId", diaryId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting diary record: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing diary record in the database.
        /// </summary>
        /// <param name="diary">The diary object with updated values.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public bool UpdateDiary(Diary diary)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.GetConnection())
                {
                    string query = @"
                        UPDATE Diaries
                        SET Content = @Content, EmotionIndex = @EmotionIndex, IsHighRisk = @IsHighRisk
                        WHERE DiaryId = @DiaryId;";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Content", diary.Content);
                        command.Parameters.AddWithValue("@EmotionIndex", diary.EmotionIndex);
                        command.Parameters.AddWithValue("@IsHighRisk", diary.IsHighRisk ? 1 : 0);
                        command.Parameters.AddWithValue("@DiaryId", diary.DiaryId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating diary record: {ex.Message}");
                return false;
            }
        }
    }
}
