using System.Text.Json;
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
        public string Name { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public int Phone { get; set; } 

        [JsonPropertyName("items")]
        public List<MyOrderItemViewModel> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);

        // Deserialize helper
        public static MyOrderViewModel FromJson(string json)
        {
            return JsonSerializer.Deserialize<MyOrderViewModel>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
    }
    public class OrderItemDTO_MVC
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string ProductImage { get; set; } = "/images/default.png";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDTOView_MVC
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDTO_MVC> Items { get; set; } = new();
        public List<UserInfoDTO_MVC> UserInfo { get; set; } = new();
    }

    public class UserInfoDTO_MVC
    {
        public string Name { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public int Phone { get; set; } = 0;
    }


}
