using Vortex_API.Repositories.Interface;
using Vortex_API.Data;
using Microsoft.EntityFrameworkCore;
using Vortex_API.Model.DTO;
namespace Vortex_API.Repositories.Service
{
    public class RevenueRepository:IRevenueRepository
    {
        private readonly AppDbContext _dbContext;

        public RevenueRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<RevenueDTO>> GetRevenueSummary(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? groupBy = "day")
        {
            fromDate ??= DateTime.UtcNow.AddMonths(-1);
            toDate ??= DateTime.UtcNow;

            var orders = await _dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == "Đã giao hàng" &&
                            o.CreatedAt >= fromDate &&
                            o.CreatedAt <= toDate)
                .ToListAsync();

            if (!orders.Any())
                return new List<RevenueDTO>();

            IEnumerable<RevenueDTO> grouped = groupBy?.ToLower() switch
            {
                "month" => orders
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new RevenueDTO
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalOrders = g.Count(),
                        TotalProductsSold = g.Sum(o => o.Items.Sum(i => i.Quantity))
                    }),

                "year" => orders
                    .GroupBy(o => o.CreatedAt.Year)
                    .Select(g => new RevenueDTO
                    {
                        Date = new DateTime(g.Key, 1, 1),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalOrders = g.Count(),
                        TotalProductsSold = g.Sum(o => o.Items.Sum(i => i.Quantity))
                    }),

                _ => orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new RevenueDTO
                    {
                        Date = g.Key,
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalOrders = g.Count(),
                        TotalProductsSold = g.Sum(o => o.Items.Sum(i => i.Quantity))
                    })
            };

            return grouped.OrderBy(x => x.Date);
        }

        public async Task<(decimal TotalRevenue, int TotalOrders, int TotalProductsSold)> GetTotalRevenue()
        {
            var totalRevenue = await _dbContext.Orders
                .Where(o => o.Status == "Đã giao hàng")
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _dbContext.Orders
                .CountAsync(o => o.Status == "Đã giao hàng");

            var totalProductsSold = await _dbContext.OrderItems
                .Where(oi => oi.Order.Status == "Đã giao hàng")
                .SumAsync(oi => oi.Quantity);

            return (totalRevenue, totalOrders, totalProductsSold);
        }
    }
}
