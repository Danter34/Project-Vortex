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
}
