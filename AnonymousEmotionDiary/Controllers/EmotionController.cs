using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// Controller for handling emotion analysis and high-risk warning display.
    /// Manages emotion detection, risk assessment, and user notifications.
    /// </summary>
    public class EmotionController
    {
        private readonly EmotionDetectionService _emotionDetectionService;
        private readonly LogService _logService;

        /// <summary>
        /// Initializes a new instance of the EmotionController class.
        /// </summary>
        public EmotionController()
        {
            _emotionDetectionService = new EmotionDetectionService();
            _logService = new LogService();
        }

        /// <summary>
        /// Handles emotion analysis for diary content.
        /// Analyzes the provided content and returns the emotion index.
        /// Logs the analysis result for tracking and debugging.
        /// </summary>
        /// <param name="content">The diary content to analyze.</param>
        /// <returns>The calculated emotion index (0-100), or -1 if analysis fails.</returns>
        public int HandleEmotionAnalysis(string content)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logService.LogDebug("Emotion analysis failed: content is empty");
                    return -1;
                }

                // Perform emotion analysis
                int emotionIndex = _emotionDetectionService.AnalyzeEmotion(content);

                // Log the analysis result
                _logService.LogEmotionAnalysis(
                    diaryId: 0,
                    emotionIndex: emotionIndex,
                    analysisTime: DateTime.Now,
                    modelVersion: "emotion-controller"
                );

                _logService.LogDebug($"Emotion analysis completed: emotion index = {emotionIndex}");

                return emotionIndex;
            }
            catch (Exception ex)
            {
                _logService.LogError("Exception during emotion analysis", ex);
                MessageBox.Show("An error occurred while analyzing emotions. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        /// <summary>
        /// Handles displaying high-risk emotion warning.
        /// Shows a warning dialog with psychological support resources if emotion index exceeds threshold.
        /// </summary>
        /// <param name="diary">The diary object containing emotion analysis results.</param>
        /// <param name="currentUser">The current logged-in user.</param>
        /// <returns>True if warning was displayed, false if emotion is not high-risk.</returns>
        public bool HandleDisplayWarning(Diary diary, User currentUser)
        {
            try
            {
                // Validate input
                if (diary == null)
                {
                    _logService.LogDebug("Display warning failed: diary is null");
                    return false;
                }

                if (currentUser == null)
                {
                    _logService.LogDebug("Display warning failed: current user is null");
                    return false;
                }

                // Check if emotion is high-risk
                if (!_emotionDetectionService.IsHighRisk(diary.EmotionIndex))
                {
                    _logService.LogDebug($"Emotion index {diary.EmotionIndex} is not high-risk, no warning displayed");
                    return false;
                }

                // Log high-risk detection
                _logService.LogDebug($"High-risk emotion detected for diary {diary.DiaryId}: emotion index = {diary.EmotionIndex}");

                // Display high-risk warning view
                HighRiskWarningView warningView = new HighRiskWarningView(diary, currentUser);
                warningView.ShowDialog();

                _logService.LogDebug($"High-risk warning displayed for diary {diary.DiaryId}");

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception while displaying warning for diary {diary?.DiaryId}", ex);
                MessageBox.Show("An error occurred while displaying the warning. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets the risk warning message for a given emotion index.
        /// Returns a formatted message with support resources.
        /// </summary>
        /// <param name="emotionIndex">The emotion index to generate warning for.</param>
        /// <returns>A formatted warning message with support resources.</returns>
        public string GetRiskWarningMessage(int emotionIndex)
        {
            try
            {
                if (!_emotionDetectionService.IsHighRisk(emotionIndex))
                {
                    _logService.LogDebug($"Emotion index {emotionIndex} is not high-risk, no warning message generated");
                    return string.Empty;
                }

                string warningMessage = _emotionDetectionService.GetRiskWarning(emotionIndex);
                _logService.LogDebug($"Risk warning message generated for emotion index {emotionIndex}");

                return warningMessage;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception while generating risk warning message for emotion index {emotionIndex}", ex);
                return string.Empty;
            }
        }
    }
}
