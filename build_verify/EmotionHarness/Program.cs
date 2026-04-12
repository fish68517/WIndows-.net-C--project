using AnonymousEmotionDiary.Services;

var service = new EmotionDetectionService();
Console.WriteLine($"NEG={service.AnalyzeEmotion("今天心情太差了 糟糕透顶")}");
Console.WriteLine($"POS={service.AnalyzeEmotion("今天天气很好，适合郊游 游玩")}");
