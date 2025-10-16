using System.Text.Json.Serialization;

namespace Vortex.Models
{
    public class MyOrderItemViewModel
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productTitle")]
        public string ProductTitle { get; set; } = string.Empty;

        [JsonPropertyName("productImage")]
        public string ProductImage { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        public decimal Subtotal => Price * Quantity;
    }

    public class MyOrderViewModel
    {
        [JsonPropertyName("id")] // map id API → OrderId
        public int OrderId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<MyOrderItemViewModel> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    }


}
