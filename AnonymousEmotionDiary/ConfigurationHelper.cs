using System;
using System.Configuration;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// Helper class for accessing application configuration settings.
    /// Provides type-safe access to configuration values with default fallbacks.
    /// </summary>
    public static class ConfigurationHelper
    {
        // Database Configuration
        public static string DatabaseConnectionString => 
            ConfigurationManager.AppSettings["DatabaseConnectionString"] ?? "Data Source=AnonymousEmotionDiary.db;Version=3;";

        public static string DatabasePath => 
            ConfigurationManager.AppSettings["DatabasePath"] ?? "AnonymousEmotionDiary.db";

        // API Configuration
        public static string LLMProvider =>
            ConfigurationManager.AppSettings["LLMProvider"] ?? "Generic";

        public static string LLMBaseUrl =>
            ConfigurationManager.AppSettings["LLMBaseUrl"] ?? "https://api.siliconflow.cn/v1";

        public static string LLMApiKey =>
            ConfigurationManager.AppSettings["LLMApiKey"] ?? string.Empty;

        public static string LLMAPIEndpoint => 
            ConfigurationManager.AppSettings["LLMAPIEndpoint"] ?? "https://api.siliconflow.cn/v1/chat/completions";

        public static string LLMAPIModel => 
            ConfigurationManager.AppSettings["LLMAPIModel"] ?? "tencent/Hunyuan-MT-7B";

        public static int LLMAPITimeout => 
            int.Parse(ConfigurationManager.AppSettings["LLMAPITimeout"] ?? "30000");

        public static int LLMUserProfileMaxChars =>
            int.Parse(ConfigurationManager.AppSettings["LLMUserProfileMaxChars"] ?? "12000");

        // Emotion Detection Configuration
        public static int EmotionHighRiskThreshold => 
            int.Parse(ConfigurationManager.AppSettings["EmotionHighRiskThreshold"] ?? "70");

        public static int EmotionIndexMin => 
            int.Parse(ConfigurationManager.AppSettings["EmotionIndexMin"] ?? "0");

        public static int EmotionIndexMax => 
            int.Parse(ConfigurationManager.AppSettings["EmotionIndexMax"] ?? "100");

        // Diary Configuration
        public static int DiaryContentMaxLength => 
            int.Parse(ConfigurationManager.AppSettings["DiaryContentMaxLength"] ?? "5000");

        public static int DiaryContentMinLength => 
            int.Parse(ConfigurationManager.AppSettings["DiaryContentMinLength"] ?? "1");

        public static int DiaryPreviewLength => 
            int.Parse(ConfigurationManager.AppSettings["DiaryPreviewLength"] ?? "100");

        // User Configuration
        public static int UsernameMinLength => 
            int.Parse(ConfigurationManager.AppSettings["UsernameMinLength"] ?? "3");

        public static int UsernameMaxLength => 
            int.Parse(ConfigurationManager.AppSettings["UsernameMaxLength"] ?? "20");

        public static int PasswordMinLength => 
            int.Parse(ConfigurationManager.AppSettings["PasswordMinLength"] ?? "8");

        public static string AdminDefaultUsername =>
            ConfigurationManager.AppSettings["AdminDefaultUsername"] ?? "admin";

        public static string AdminDefaultPassword =>
            ConfigurationManager.AppSettings["AdminDefaultPassword"] ?? "Admin@12345";

        // Logging Configuration
        public static string LogFilePath => 
            ConfigurationManager.AppSettings["LogFilePath"] ?? "Logs";

        public static string LogLevel => 
            ConfigurationManager.AppSettings["LogLevel"] ?? "Debug";

        public static int LogMaxFileSizeMB => 
            int.Parse(ConfigurationManager.AppSettings["LogMaxFileSizeMB"] ?? "10");

        public static string DebugLogFileName => 
            ConfigurationManager.AppSettings["DebugLogFileName"] ?? "debug.log";

        public static string ErrorLogFileName => 
            ConfigurationManager.AppSettings["ErrorLogFileName"] ?? "error.log";

        public static string EmotionAnalysisLogFileName => 
            ConfigurationManager.AppSettings["EmotionAnalysisLogFileName"] ?? "emotion_analysis.log";
    }
}
