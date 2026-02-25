using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// 用于处理情绪分析与高风险预警展示的控制器。
    /// 负责情绪检测、风险评估以及用户提示通知。
    /// </summary>
    public class EmotionController
    {
        private readonly EmotionDetectionService _emotionDetectionService;
        private readonly LogService _logService;

        /// <summary>
        /// 初始化 EmotionController 类的新实例。
        /// </summary>
        public EmotionController()
        {
            _emotionDetectionService = new EmotionDetectionService();
            _logService = new LogService();
        }

        public int HandleEmotionAnalysis(string content)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logService.LogDebug("情绪分析失败：内容为空");
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

                _logService.LogDebug($"情绪分析完成：情绪指数={emotionIndex}");

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
                    _logService.LogDebug("展示预警失败：diary 为空");
                    return false;
                }

                if (currentUser == null)
                {
                    _logService.LogDebug("展示预警失败：当前用户为空");
                    return false;
                }

                // Check if emotion is high-risk
                if (!_emotionDetectionService.IsHighRisk(diary.EmotionIndex))
                {
                    _logService.LogDebug($"情绪指数 {diary.EmotionIndex} 未达到高风险阈值，不显示预警");
                    return false;
                }

                // Log high-risk detection
                _logService.LogDebug($"检测到高风险情绪：DiaryId={diary.DiaryId}，情绪指数={diary.EmotionIndex}");

                // Display high-risk warning view
                HighRiskWarningView warningView = new HighRiskWarningView(diary, currentUser);
                warningView.ShowDialog();

                _logService.LogDebug($"已展示高风险预警：DiaryId={diary.DiaryId}");

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
                    _logService.LogDebug($"情绪指数 {emotionIndex} 未达到高风险阈值，不生成预警文案");
                    return string.Empty;
                }

                string warningMessage = _emotionDetectionService.GetRiskWarning(emotionIndex);
                _logService.LogDebug($"已生成预警文案：情绪指数={emotionIndex}");

                return warningMessage;
            }
            catch (Exception ex)
            {
                // _logService.LogDebug($"生成预警文案时发生异常：情绪指数={emotionIndex}", ex);
                return string.Empty;
            }
        }
    }
}
