using System;
using System.Collections.Generic;
using System.Linq;
using AnonymousEmotionDiary.DAOs;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Administrative service for cross-user emotion monitoring and follow-up records.
    /// </summary>
    public class AdminService
    {
        private readonly UserService _userService;
        private readonly DiaryService _diaryService;
        private readonly IEmotionDetectionService _emotionDetectionService;
        private readonly AdminContactDAO _adminContactDAO;
        private readonly LogService _logService;

        public AdminService()
        {
            _userService = new UserService();
            _diaryService = new DiaryService();
            _emotionDetectionService = new EmotionDetectionService();
            _adminContactDAO = new AdminContactDAO();
            _logService = new LogService();
        }

        public List<UserEmotionProfile> GetAllUserEmotionProfiles()
        {
            try
            {
                List<User> users = _userService.GetAllRegularUsers();
                List<Diary> allDiaries = _diaryService.GetAllDiaries();
                List<UserEmotionProfile> profiles = new List<UserEmotionProfile>();

                foreach (User user in users)
                {
                    List<Diary> userDiaries = allDiaries
                        .Where(d => d.UserId == user.UserId)
                        .OrderByDescending(d => d.CreatedAt)
                        .ToList();

                    UserEmotionProfile profile = _emotionDetectionService.AnalyzeUserEmotionProfile(user, userDiaries);
                    AdminContactRecord latestContact = _adminContactDAO.SelectLatestContactByUserId(user.UserId);
                    profile.LastContactAt = latestContact?.CreatedAt;
                    profiles.Add(profile);
                }

                return profiles
                    .OrderByDescending(p => p.OverallEmotionIndex)
                    .ThenByDescending(p => p.HighRiskCount)
                    .ThenByDescending(p => p.LastDiaryAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError("Error retrieving emotion profiles for administrator.", ex);
                return new List<UserEmotionProfile>();
            }
        }

        public UserEmotionProfile GetUserEmotionProfile(User user)
        {
            List<Diary> diaries = _diaryService.GetUserDiaries(user.UserId);
            UserEmotionProfile profile = _emotionDetectionService.AnalyzeUserEmotionProfile(user, diaries);
            profile.LastContactAt = _adminContactDAO.SelectLatestContactByUserId(user.UserId)?.CreatedAt;
            return profile;
        }

        public bool RecordContact(User adminUser, User targetUser, int emotionIndex, string contactMethod, string note)
        {
            try
            {
                AdminContactRecord record = new AdminContactRecord
                {
                    UserId = targetUser.UserId,
                    AdminUserId = adminUser.UserId,
                    Username = targetUser.Username,
                    AdminUsername = adminUser.Username,
                    EmotionIndexSnapshot = emotionIndex,
                    ContactMethod = string.IsNullOrWhiteSpace(contactMethod) ? "人工联系" : contactMethod.Trim(),
                    ContactNote = string.IsNullOrWhiteSpace(note) ? "管理员已查看并记录跟进。" : note.Trim(),
                    CreatedAt = DateTime.Now
                };

                bool success = _adminContactDAO.InsertContactRecord(record);
                if (success)
                {
                    _logService.LogDebug($"Admin {adminUser.Username} recorded contact for user {targetUser.Username}.");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error recording administrator contact.", ex);
                return false;
            }
        }

        public List<AdminContactRecord> GetContactHistory(int userId)
        {
            return _adminContactDAO.SelectContactsByUserId(userId);
        }
    }
}
