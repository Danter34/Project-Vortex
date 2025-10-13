using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RevenueController : ControllerBase
    {
       private readonly IRevenueRepository _revenueRepository;
        public RevenueController(IRevenueRepository revenueRepository)
        {
            _revenueRepository = revenueRepository;
        }

        [HttpGet("summary")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetRevenueSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? groupBy = "day")
        {
            var data = await _revenueRepository.GetRevenueSummary(fromDate, toDate, groupBy);
            return Ok(data);
        }

        [HttpGet("total")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var (totalRevenue, totalOrders, totalProductsSold) = await _revenueRepository.GetTotalRevenue();
            return Ok(new
            {
                totalRevenue,
                totalOrders,
                totalProductsSold
            });
        }
    }
}
