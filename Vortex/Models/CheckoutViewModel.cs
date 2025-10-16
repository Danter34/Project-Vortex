namespace Vortex.Models
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal ShippingFee => 0;
        public decimal GrandTotal => Subtotal + ShippingFee;

        // Shipping info
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD"; // default
    }
}
