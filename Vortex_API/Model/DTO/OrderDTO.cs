namespace Vortex_API.Model.DTO
{
    public class OrderDTO
    {
        public List<CartItemDTO> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "COD";
        public string ShippingAddress { get; set; }
        public string Name { get; set; }
        public int Phone { get; set; }
    }
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string ProductImage { get; set; } = "/images/default.png";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class OrderDTOView
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDTO> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public List<OrderDTO> UserInfo { get; set; } = new();
    }

}
