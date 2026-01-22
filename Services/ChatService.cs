namespace TourismPlatform.Services
{
    public class ChatService : IChatService
    {
        private readonly Dictionary<string, string> _faqDatabase;

        public ChatService()
        {
            _faqDatabase = new Dictionary<string, string>
            {
                { "开放时间", "大多数景点的开放时间为上午9点到下午5点，具体时间请查看景点详情页面。" },
                { "门票价格", "门票价格因景点而异，请在景点详情页面查看具体价格。" },
                { "如何购票", "您可以在景点详情页面点击'购买门票'按钮进行购票。" },
                { "如何预订酒店", "在景点详情页面或酒店列表页面选择酒店，然后选择房型和日期进行预订。" },
                { "支付方式", "目前支持模拟支付，实际应用中可集成真实支付方式。" },
                { "核销码", "订单支付成功后会生成核销码，用于景点入场或酒店入住时核销。" },
                { "如何发布游记", "登录后访问'发布游记'页面，填写标题、内容并上传图片即可。" },
                { "如何收藏景点", "在景点详情页面点击'收藏'按钮即可收藏该景点。" },
                { "如何评论", "订单使用完成后，可在景点详情页面进行评分和评论。" },
                { "天气预报", "访问'天气预报'页面可查看未来几天的天气情况。" }
            };
        }

        public async Task<string> GetAnswerAsync(string question)
        {
            // Simple keyword matching
            foreach (var faq in _faqDatabase)
            {
                if (question.Contains(faq.Key))
                {
                    return await Task.FromResult(faq.Value);
                }
            }

            // Default response if no match found
            return await Task.FromResult("抱歉，我没有找到相关答案。请尝试其他问题或联系客服。");
        }
    }
}
