using System;

namespace AnonymousEmotionDiary.Models
{
    /// <summary>
    /// Represents one administrator follow-up action for a user.
    /// </summary>
    public class AdminContactRecord
    {
        public int ContactId { get; set; }

        public int UserId { get; set; }

        public int AdminUserId { get; set; }

        public string Username { get; set; }

        public string AdminUsername { get; set; }

        public int EmotionIndexSnapshot { get; set; }

        public string ContactMethod { get; set; }

        public string ContactNote { get; set; }

        public DateTime CreatedAt { get; set; }

        public AdminContactRecord()
        {
            Username = string.Empty;
            AdminUsername = string.Empty;
            ContactMethod = string.Empty;
            ContactNote = string.Empty;
            CreatedAt = DateTime.Now;
        }
    }
}
