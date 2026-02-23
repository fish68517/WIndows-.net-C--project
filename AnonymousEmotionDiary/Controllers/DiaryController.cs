using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// Controller for handling diary management operations.
    /// Manages diary creation, viewing, deletion, and detail display flows.
    /// </summary>
    public class DiaryController
    {
        private readonly DiaryService _diaryService;
        private readonly LogService _logService;

        /// <summary>
        /// Initializes a new instance of the DiaryController class.
        /// </summary>
        public DiaryController()
        {
            _diaryService = new DiaryService();
            _logService = new LogService();
        }

        /// <summary>
        /// Handles diary creation.
        /// Validates content, calls DiaryService to create diary, and displays appropriate messages.
        /// </summary>
        /// <param name="userId">The user ID of the diary owner.</param>
        /// <param name="content">The content of the diary entry.</param>
        /// <returns>The created Diary object if successful, null otherwise.</returns>
        public Diary HandleCreateDiary(int userId, string content)
        {
            try
            {
                // Validate input is not empty
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logService.LogDebug($"Diary creation failed for user {userId}: empty content");
                    MessageBox.Show("Please enter diary content.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Validate content length
                if (!_diaryService.ValidateDiaryContent(content))
                {
                    _logService.LogDebug($"Diary creation failed for user {userId}: invalid content length");
                    MessageBox.Show("Diary content must not exceed 5000 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Attempt to create diary
                Diary createdDiary = _diaryService.CreateDiary(userId, content);

                if (createdDiary != null)
                {
                    _logService.LogDebug($"Diary creation successful for user {userId}: DiaryId {createdDiary.DiaryId}, EmotionIndex {createdDiary.EmotionIndex}");
                    MessageBox.Show("Diary published successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return createdDiary;
                }
                else
                {
                    _logService.LogDebug($"Diary creation failed for user {userId}: DiaryService returned null");
                    MessageBox.Show("Failed to publish diary. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception during diary creation for user {userId}", ex);
                MessageBox.Show("An unexpected error occurred while creating the diary. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Handles retrieving and displaying user's diary list.
        /// Fetches all diaries for the current user and displays them in the diary list view.
        /// </summary>
        /// <param name="userId">The user ID to retrieve diaries for.</param>
        /// <returns>A list of Diary objects for the user, or an empty list if none found.</returns>
        public List<Diary> HandleViewDiaries(int userId)
        {
            try
            {
                List<Diary> userDiaries = _diaryService.GetUserDiaries(userId);
                _logService.LogDebug($"Retrieved {userDiaries.Count} diaries for user {userId}");
                return userDiaries;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception while retrieving diaries for user {userId}", ex);
                MessageBox.Show("An error occurred while loading your diaries. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Diary>();
            }
        }

        /// <summary>
        /// Handles diary deletion.
        /// Deletes the specified diary from the database and refreshes the diary list.
        /// </summary>
        /// <param name="diaryId">The diary ID to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public bool HandleDeleteDiary(int diaryId)
        {
            try
            {
                // Confirm deletion with user
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this diary? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    _logService.LogDebug($"Diary deletion cancelled by user for DiaryId {diaryId}");
                    return false;
                }

                // Attempt to delete diary
                bool deleteSuccess = _diaryService.DeleteDiary(diaryId);

                if (deleteSuccess)
                {
                    _logService.LogDebug($"Diary deleted successfully: DiaryId {diaryId}");
                    MessageBox.Show("Diary deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    _logService.LogDebug($"Diary deletion failed: DiaryId {diaryId} not found or deletion unsuccessful");
                    MessageBox.Show("Failed to delete diary. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception during diary deletion for DiaryId {diaryId}", ex);
                MessageBox.Show("An unexpected error occurred while deleting the diary. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Handles displaying diary detail view.
        /// Retrieves the specified diary and displays its complete content, emotion index, and metadata.
        /// </summary>
        /// <param name="diaryId">The diary ID to display.</param>
        /// <returns>The Diary object if found and displayed, null otherwise.</returns>
        public Diary HandleViewDiaryDetail(int diaryId)
        {
            try
            {
                // Retrieve diary
                Diary diary = _diaryService.GetDiaryById(diaryId);

                if (diary == null)
                {
                    _logService.LogDebug($"Diary detail view failed: DiaryId {diaryId} not found");
                    MessageBox.Show("Diary not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Display diary detail
                DisplayDiaryDetail(diary);
                _logService.LogDebug($"Diary detail displayed for DiaryId {diaryId}");
                return diary;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception while displaying diary detail for DiaryId {diaryId}", ex);
                MessageBox.Show("An error occurred while loading diary details. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Displays the diary detail information in a message box.
        /// Shows complete content, creation time, emotion index, and high-risk warning if applicable.
        /// </summary>
        /// <param name="diary">The diary object to display.</param>
        private void DisplayDiaryDetail(Diary diary)
        {
            // Build detail message
            string detailMessage = $"Diary ID: {diary.DiaryId}\n" +
                                  $"Date: {diary.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"Emotion Index: {diary.EmotionIndex}\n" +
                                  $"---\n" +
                                  $"{diary.Content}";

            // Add high-risk warning if applicable
            if (diary.IsHighRisk)
            {
                detailMessage += "\n\n⚠ HIGH RISK EMOTION DETECTED\n" +
                               "Please consider reaching out for support:\n" +
                               "- Campus Counseling Center\n" +
                               "- Mental Health Hotline: 1-800-XXX-XXXX\n" +
                               "- Crisis Text Line: Text HOME to 741741";
            }

            MessageBox.Show(detailMessage, "Diary Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
