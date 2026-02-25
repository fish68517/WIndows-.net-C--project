using System;

namespace AnonymousEmotionDiary.Models
{
   
    public class Diary
    {
     
        public int DiaryId { get; set; }

      
        public int UserId { get; set; }

      
        public string Content { get; set; }

      
        public int EmotionIndex { get; set; }

    
        public bool IsHighRisk { get; set; }

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
