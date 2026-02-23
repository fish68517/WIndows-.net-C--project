using System;

namespace AnonymousEmotionDiary.Models
{
    /// <summary>
    /// Represents a diary entry in the system.
    /// Contains diary content, emotion analysis results, and metadata.
    /// </summary>
    public class Diary
    {
        /// <summary>
        /// Gets or sets the unique identifier for the diary entry.
        /// </summary>
        public int DiaryId { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the diary owner.
        /// References the Users table.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the content of the diary entry.
        /// Must not be empty and not exceed 5000 characters.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the emotion index for the diary entry.
        /// Range: 0-100, where higher values indicate more negative emotions.
        /// </summary>
        public int EmotionIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the diary entry is marked as high risk.
        /// High risk is determined when emotion index exceeds 70.
        /// </summary>
        public bool IsHighRisk { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the diary entry was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the Diary class.
        /// </summary>
        public Diary()
        {
            CreatedAt = DateTime.Now;
            IsHighRisk = false;
        }

        /// <summary>
        /// Initializes a new instance of the Diary class with specified parameters.
        /// </summary>
        /// <param name="userId">The user ID of the diary owner.</param>
        /// <param name="content">The content of the diary entry.</param>
        /// <param name="emotionIndex">The emotion index for the entry.</param>
        public Diary(int userId, string content, int emotionIndex)
        {
            UserId = userId;
            Content = content;
            EmotionIndex = emotionIndex;
            IsHighRisk = emotionIndex > 70;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the diary entry.
        /// </summary>
        /// <returns>A formatted string containing diary information.</returns>
        public override string ToString()
        {
            return $"Diary: ID={DiaryId}, UserId={UserId}, EmotionIndex={EmotionIndex}, " +
                   $"HighRisk={IsHighRisk}, Created={CreatedAt:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
