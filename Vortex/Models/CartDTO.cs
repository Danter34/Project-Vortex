namespace Vortex.Models
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal ShippingFee => 0; // miễn phí
        public decimal GrandTotal => Subtotal + ShippingFee;
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal => Price * Quantity;

        // Chuyển từ ProductViewModel + số lượng -> CartItemViewModel
        public static CartItemViewModel FromProduct(ProductViewModel product, int quantity)
        {
            return new CartItemViewModel
            {
                ProductId = product.Id,
                ProductTitle = product.Title,
                ProductImage = product.Images.FirstOrDefault()?.FilePath ?? "/images/default.png",
                Price = product.Price,
                Quantity = quantity
            };
        }
    }
}
