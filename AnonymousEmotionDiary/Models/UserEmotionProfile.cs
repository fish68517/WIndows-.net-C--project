using System;

namespace AnonymousEmotionDiary.Models
{
    /// <summary>
    /// Aggregated emotional analysis result for one user's diary history.
    /// </summary>
    public class UserEmotionProfile
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string ContactInfo { get; set; }

        public int DiaryCount { get; set; }

        public int HighRiskCount { get; set; }

        public int LatestEmotionIndex { get; set; }

        public int AverageEmotionIndex { get; set; }

        public int OverallEmotionIndex { get; set; }

        public string RiskLevel { get; set; }

        public string Summary { get; set; }

        public string SuggestedAction { get; set; }

        public DateTime? LastDiaryAt { get; set; }

        public DateTime? LastContactAt { get; set; }

        public UserEmotionProfile()
        {
            Username = string.Empty;
            ContactInfo = string.Empty;
            RiskLevel = "未知";
            Summary = string.Empty;
            SuggestedAction = string.Empty;
        }
    }
}
