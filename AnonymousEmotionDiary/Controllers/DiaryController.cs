using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// 用于处理日记管理操作的控制器。
    /// 负责日记的创建、查看、删除以及详情展示流程。
    /// </summary>
    public class DiaryController
    {
        private readonly DiaryService _diaryService;
        private readonly LogService _logService;

        /// <summary>
        /// 初始化 DiaryController 类的新实例。
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
                    _logService.LogDebug($"用户 {userId} 发布日记失败：内容为空");
                    MessageBox.Show("请输入日记内容。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Validate content length
                if (!_diaryService.ValidateDiaryContent(content))
                {
                    _logService.LogDebug($"用户 {userId} 发布日记失败：内容长度不合法");
                    MessageBox.Show("日记内容不能超过 5000 个字符。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Attempt to create diary
                Diary createdDiary = _diaryService.CreateDiary(userId, content);

                if (createdDiary != null)
                {
                    _logService.LogDebug($"用户 {userId} 发布日记成功：DiaryId={createdDiary.DiaryId}，情绪指数={createdDiary.EmotionIndex}");
                    MessageBox.Show("日记发布成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return createdDiary;
                }
                else
                {
                    _logService.LogDebug($"用户 {userId} 发布日记失败：DiaryService 返回空对象");
                    MessageBox.Show("日记发布失败，请重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _logService.LogDebug($"已为用户 {userId} 获取到 {userDiaries.Count} 篇日记");
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
                    "确定要删除这篇日记吗？此操作不可撤销。",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    _logService.LogDebug($"用户取消删除日记：DiaryId={diaryId}");
                    return false;
                }

                // Attempt to delete diary
                bool delete成功 = _diaryService.DeleteDiary(diaryId);

                if (delete成功)
                {
                    _logService.LogDebug($"日记删除成功：DiaryId={diaryId}");
                    MessageBox.Show("日记已删除。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    _logService.LogDebug($"日记删除失败：DiaryId={diaryId} 不存在或删除未成功");
                    MessageBox.Show("删除日记失败，请重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _logService.LogDebug($"已展示日记详情：DiaryId={diaryId}");
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
            string detailMessage = $"日记ID：{diary.DiaryId}\n" +
                                  $"日期：{diary.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"情绪指数：{diary.EmotionIndex}\n" +
                                  $"---\n" +
                                  $"{diary.Content}";

            // Add high-risk warning if applicable
            if (diary.IsHighRisk)
            {
                detailMessage += "\n\n⚠ 检测到高风险情绪\n" +
                               "建议你寻求帮助支持：\n" +
                               "- 校园心理咨询中心\n" +
                               "- 心理健康热线：1-800-XXX-XXXX\n" +
                               "- 危机短信热线：发送 HOME 到 741741";
            }

            MessageBox.Show(detailMessage, "日记详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
