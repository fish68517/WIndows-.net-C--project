namespace TourismPlatform.Services.Alipay
{
    public interface IAlipayService
    {
        /// <summary>
        /// 生成支付跳转的 HTML 表单（电脑网站支付）
        /// </summary>
        string GeneratePagePayRequest(string outTradeNo, string totalAmount, string subject, string returnUrl);

        /// <summary>
        /// 验证支付宝回调签名
        /// </summary>
        bool ValidateCallback(Dictionary<string, string> paramsMap);
    }
}