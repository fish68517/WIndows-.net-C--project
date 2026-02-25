using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.DAOs
{

    public class DiaryDAO
    {
      
        public bool InsertDiary(Diary diary)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

          public Diary SelectDiaryById(int diaryId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

      
        public List<Diary> SelectDiariesByUserId(int userId)
        {
            List<Diary> diaries = new List<Diary>();

            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

          public bool DeleteDiary(int diaryId)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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

        public bool UpdateDiary(Diary diary)
        {
            try
            {
                using (SQLiteConnection connection = DatabaseManager.CreateConnection())
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
