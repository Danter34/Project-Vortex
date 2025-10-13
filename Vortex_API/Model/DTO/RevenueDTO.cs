namespace Vortex_API.Model.DTO
{
    public class RevenueDTO
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
    }
}
