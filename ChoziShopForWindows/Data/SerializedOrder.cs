using ChoziShop.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace ChoziShopForWindows.Data
{
    public class SerializedOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("store_id")]
        public long StoreId { get; set; }
        [JsonProperty("preferred_payment_mode")]
        public int PreferredPaymentMode { get; set; }
        [JsonProperty("order_status")]
        public int OrderStatus { get; set; }
        [JsonProperty("payment_status")]
        public int PaymentStatus { get; set; }
        [JsonProperty("total_amount_cents")]
        public decimal TotalAmountCents { get; set; }
        [JsonProperty("currency")]
        public required string Currency {  get; set; }
        
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("order_category_products")]

        public required string OrderCategoryProducts { get; set; }
        [JsonProperty("offline_updated_at")]
        public DateTime OfflineUpdatedAt { get; set; }

    }
}
