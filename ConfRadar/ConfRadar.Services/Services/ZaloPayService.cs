using ConfRadar.Services.Common;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IZaloPayService
    {
        Task<string> CreateMomoPayment();
    }
    public class ZaloPayService : IZaloPayService
    {
        private readonly IOptions<ZaloPaySettings> _zaloPaySettings;
        private readonly ITokenService _tokenService;
        public ZaloPayService(IOptions<ZaloPaySettings> zaloPaySettings, ITokenService tokenService)
        {
            _zaloPaySettings = zaloPaySettings;
            _tokenService = tokenService;
        }
        public async Task<string> CreateMomoPayment()
        {
            string createOrderUrl = "https://sb-openapi.zalopay.vn/v2/create";
            var vnTime = ExtensionHelper.GetVietnamTime();
            string app_trans_id = $"{vnTime:yyMMdd}_{Guid.NewGuid().ToString("N")[..6]}";
            string appUser = "ZaloPayDemo";
            long app_time = new DateTimeOffset(vnTime).ToUnixTimeMilliseconds();
            long amount = 50000;
            var items = new List<ZaloPayItem>
            {
                new ZaloPayItem
                {
            ItemId = "knb",
            ItemName = "kim nguyen bao",
            ItemPrice = 198400,
            ItemQuantity = 1
                }
            };
            string itemJsonString = JsonConvert.SerializeObject(items);
            var embedData = new ZaloPayEmbedData
            {
                PromotionInfo = "",
                MerchantInfo = "du lieu rieng cua ung dung"
            };

            string embedDataJson = JsonConvert.SerializeObject(embedData);
            string hmacInput = $"{_zaloPaySettings.Value.AppId}|{app_trans_id}|{appUser}|{amount}|{app_time}|{embedDataJson}|{itemJsonString}";

            var mac = _tokenService.CreateSignature(hmacInput, _zaloPaySettings.Value.Key1);
            var bodyRequest = new
            {
                app_id = _zaloPaySettings.Value.AppId,
                app_user = appUser,
                app_trans_id = app_trans_id,
                app_time = app_time,
                amount = amount,
                item = itemJsonString,
                description = "Thanh toán đơn hàng confradar",
                embed_data = embedDataJson,
                mac = mac,
                callback_url = "https://483833826218.ngrok-free.app/api/Payment/test-zalo-pay",
            };
            using (var httpClient = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(bodyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(createOrderUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                return responseBody;
            }
        }
        public class ZaloPayItem
        {
            [JsonProperty("itemid")]
            public string ItemId { get; set; }

            [JsonProperty("itemname")]
            public string ItemName { get; set; }

            [JsonProperty("itemprice")]
            public long ItemPrice { get; set; }

            [JsonProperty("itemquantity")]
            public int ItemQuantity { get; set; }
        }
        public class ZaloPayEmbedData
        {
            [JsonProperty("promotioninfo")]
            public string PromotionInfo { get; set; } = string.Empty;

            [JsonProperty("merchantinfo")]
            public string MerchantInfo { get; set; } = string.Empty;
        }
    }
}
