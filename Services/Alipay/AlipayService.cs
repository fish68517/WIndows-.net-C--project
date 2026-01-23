using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Util;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TourismPlatform.Services.Alipay
{
    public class AlipayService : IAlipayService
    {
        private readonly IConfiguration _configuration;
        private readonly IAopClient _client;

        public AlipayService(IConfiguration configuration)
        {
            _configuration = configuration;

            var config = _configuration.GetSection("Alipay");

            _client = new DefaultAopClient(
                config["GatewayUrl"],
                config["AppId"],
                config["PrivateKey"],
                "json",
                "1.0",
                "RSA2",
                config["AlipayPublicKey"],
                "UTF-8",
                false
            );
        }

        public string GeneratePagePayRequest(string outTradeNo, string totalAmount, string subject, string returnUrl)
        {
            var request = new AlipayTradePagePayRequest();

            request.SetReturnUrl(returnUrl);

            // ✅ 用 AopObject 模型，而不是匿名对象
            var model = new AlipayTradePagePayModel
            {
                OutTradeNo = outTradeNo,
                ProductCode = "FAST_INSTANT_TRADE_PAY",
                TotalAmount = totalAmount,
                Subject = subject,
                Body = "旅游平台门票预订"
            };
            request.SetBizModel(model);

            // ✅ 你的 SDK 很可能是 pageExecute（小写 p）
            var response = _client.pageExecute(request);
            return response.Body;
        }

        public bool ValidateCallback(Dictionary<string, string> paramsMap)
        {
            if (paramsMap == null || paramsMap.Count == 0) return false;

            var config = _configuration.GetSection("Alipay");

            // ✅ AlipaySignature 在 Aop.Api.Util
            return AlipaySignature.RSACheckV1(
                paramsMap,
                config["AlipayPublicKey"],
                "UTF-8",
                "RSA2",
                false
            );
        }
    }
}
