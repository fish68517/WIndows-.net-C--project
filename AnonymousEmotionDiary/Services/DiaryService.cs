using System;
using System.Collections.Generic;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.DAOs;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Service for managing diary operations.
    /// Handles diary creation, retrieval, deletion, and validation logic.
    /// </summary>
    public class DiaryService
    {
        private readonly DiaryDAO _diaryDAO;
        private readonly LogService _logService;
        private readonly IEmotionDetectionService _emotionDetectionService;

        // Constants for validation
        private const int MaxDiaryContentLength = 5000;

        /// <summary>
        /// Initializes a new instance of the DiaryService class.
        /// </summary>
        /// <param name="emotionDetectionService">The emotion detection service to use for analyzing diary content.</param>
        public DiaryService(IEmotionDetectionService emotionDetectionService = null)
        {
            _diaryDAO = new DiaryDAO();
            _logService = new LogService();
            _emotionDetectionService = emotionDetectionService;
        }

        /// <summary>
        /// Validates the diary content according to business rules.
        /// Content must not be empty and not exceed 5000 characters.
        /// </summary>
        /// <param name="content">The diary content to validate.</param>
        /// <returns>True if the content is valid, false otherwise.</returns>
        public bool ValidateDiaryContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logService.LogDebug("Diary content validation failed: content is null or empty");
                return false;
            }

            // Check maximum length
            if (content.Length > MaxDiaryContentLength)
            {
                _logService.LogDebug($"Diary content validation failed: length {content.Length} exceeds maximum {MaxDiaryContentLength}");
                return false;
            }

            _logService.LogDebug($"Diary content validation passed: length {content.Length}");
            return true;
        }

        /// <summary>
        /// Creates a new diary entry with emotion analysis.
        /// Validates content, analyzes emotion, determines high-risk status, and saves to database.
        /// </summary>
        /// <param name="userId">The user ID of the diary owner.</param>
        /// <param name="content">The content of the diary entry.</param>
        /// <returns>The created Diary object if successful, null otherwise.</returns>
        public Diary CreateDiary(int userId, string content)
        {
            try
            {
                // Validate diary content
                if (!ValidateDiaryContent(content))
                {
                    _logService.LogDebug($"Diary creation failed: invalid content for user {userId}");
                    return null;
                }

                // Analyze emotion
                int emotionIndex = 0;
                if (_emotionDetectionService != null)
                {
                    emotionIndex = _emotionDetectionService.AnalyzeEmotion(content);
                    _logService.LogDebug($"Emotion analysis completed for user {userId}: emotion index = {emotionIndex}");
                }
                else
                {
                    _logService.LogDebug($"Emotion detection service not available, using default emotion index 0");
                }

                // Create diary object
                Diary diary = new Diary(userId, content, emotionIndex);

                // Insert into database
                bool insertSuccess = _diaryDAO.InsertDiary(diary);
                if (!insertSuccess)
                {
                    _logService.LogDebug($"Diary creation failed: database insertion failed for user {userId}");
                    return null;
                }

                // Retrieve the created diary to get the assigned DiaryId
                List<Diary> userDiaries = _diaryDAO.SelectDiariesByUserId(userId);
                if (userDiaries.Count > 0)
                {
                    Diary createdDiary = userDiaries[0]; // Most recent diary (ordered DESC)
                    _logService.LogDebug($"Diary creation successful: user {userId}, DiaryId: {createdDiary.DiaryId}, EmotionIndex: {emotionIndex}, HighRisk: {createdDiary.IsHighRisk}");
                    return createdDiary;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error during diary creation for user {userId}", ex);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a diary entry by diary ID.
        /// </summary>
        /// <param name="diaryId">The diary ID to search for.</param>
        /// <returns>The Diary object if found, null otherwise.</returns>
        public Diary GetDiaryById(int diaryId)
        {
            try
            {
                Diary diary = _diaryDAO.SelectDiaryById(diaryId);
                if (diary != null)
                {
                    _logService.LogDebug($"Diary retrieved by ID: {diaryId}");
                }
                else
                {
                    _logService.LogDebug($"Diary not found for ID: {diaryId}");
                }
                return diary;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error retrieving diary by ID: {diaryId}", ex);
                return null;
            }
        }

        /// <summary>
        /// Retrieves all diary entries for a specific user.
        /// Diaries are ordered by creation time in descending order (newest first).
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>A list of Diary objects for the user, or an empty list if none found.</returns>
        public List<Diary> GetUserDiaries(int userId)
        {
            try
            {
                List<Diary> diaries = _diaryDAO.SelectDiariesByUserId(userId);
                _logService.LogDebug($"Retrieved {diaries.Count} diaries for user {userId}");
                return diaries;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error retrieving diaries for user {userId}", ex);
                return new List<Diary>();
            }
        }

        /// <summary>
        /// Deletes a diary entry from the database.
        /// </summary>
        /// <param name="diaryId">The diary ID to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public bool DeleteDiary(int diaryId)
        {
            try
            {
                bool deleteSuccess = _diaryDAO.DeleteDiary(diaryId);
                if (deleteSuccess)
                {
                    _logService.LogDebug($"Diary deleted successfully: DiaryId {diaryId}");
                }
                else
                {
                    _logService.LogDebug($"Diary deletion failed: DiaryId {diaryId} not found or deletion unsuccessful");
                }
                return deleteSuccess;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error deleting diary: {diaryId}", ex);
                return false;
            }
        }
    }
}
