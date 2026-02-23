namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Interface for emotion detection service.
    /// Defines the contract for analyzing diary content and generating emotion indices.
    /// </summary>
    public interface IEmotionDetectionService
    {
        /// <summary>
        /// Analyzes the emotion in the provided content and returns an emotion index.
        /// </summary>
        /// <param name="content">The diary content to analyze.</param>
        /// <returns>An emotion index value between 0 and 100, where higher values indicate more negative emotions.</returns>
        int AnalyzeEmotion(string content);

        /// <summary>
        /// Determines if an emotion index indicates high-risk emotional state.
        /// </summary>
        /// <param name="emotionIndex">The emotion index to check.</param>
        /// <returns>True if emotion index exceeds the high-risk threshold, false otherwise.</returns>
        bool IsHighRisk(int emotionIndex);

        /// <summary>
        /// Generates a risk warning message for high-risk emotions.
        /// Includes psychological support resources.
        /// </summary>
        /// <param name="emotionIndex">The emotion index that triggered the warning.</param>
        /// <returns>A warning message with support resources.</returns>
        string GetRiskWarning(int emotionIndex);
    }
}
