using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Vortex_API.Repositories.Service
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByProduct(int productId)
        {
            return await _context.Reviews
                .Include(r => r.User) // navigation property tới ApplicationUser
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDTO
                {
                    ProductId = r.ProductId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    FullName = r.User.FullName
                })
                .ToListAsync();
        }

        public async Task<Review?> AddReview(string userId, ReviewDTO dto)
        {
            //  checking order status
            var hasBought = await _context.Orders
                .Include(o => o.Items)
                .AnyAsync(o =>
                    o.UserId == userId &&
                    o.Status == "Đã giao hàng" &&
                    o.Items.Any(i => i.ProductId == dto.ProductId));

            if (!hasBought)
                throw new Exception("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua hàng.");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // update product rate
            var product = await _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product != null)
            {
                product.AverageRating = product.Reviews.Average(r => r.Rating);
                await _context.SaveChangesAsync();
            }

            return review;
        }
    }
}
