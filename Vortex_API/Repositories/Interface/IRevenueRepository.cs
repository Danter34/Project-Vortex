using Vortex_API.Model.DTO;
namespace Vortex_API.Repositories.Interface
{
    public interface IRevenueRepository
    {
        Task<IEnumerable<RevenueDTO>> GetRevenueSummary(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? groupBy = "day");

        Task<(decimal TotalRevenue, int TotalOrders, int TotalProductsSold)> GetTotalRevenue();
    }
}
