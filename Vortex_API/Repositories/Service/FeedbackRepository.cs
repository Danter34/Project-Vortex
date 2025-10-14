using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using Vortex_API.Data;
using Microsoft.EntityFrameworkCore;
namespace Vortex_API.Repositories.Service
{
    public class FeedbackRepository: IFeedbackRepository
    {
        private readonly AppDbContext _context;

        public FeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeedbackResponseDTO?> CreateFeedback(string userId, FeedbackDTO dto)
        {
            //  Kiểm tra user có gửi trong vòng 24h chưa
            var lastFeedback = await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastFeedback != null && (DateTime.Now - lastFeedback.CreatedAt).TotalHours < 24)
            {
                return null; // Không được gửi trong vòng 24h
            }

            var feedback = new Feedback
            {
                UserId = userId,
                Message = dto.Message
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return new FeedbackResponseDTO
            {
                Id = feedback.Id,
                Message = feedback.Message,
                AdminReply = feedback.AdminReply,
                CreatedAt = feedback.CreatedAt,
                Status = feedback.Status
            };
        }

        public async Task<FeedbackResponseDTO?> GetFeedbackByUser(string userId)
        {
            var feedback = await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            return new FeedbackResponseDTO
            {
                Id = feedback.Id,
                Message = feedback.Message,
                AdminReply = feedback.AdminReply,
                CreatedAt = feedback.CreatedAt,
                Status = feedback.Status
            };
        }

        public async Task<List<FeedbackResponseDTO>> GetAllFeedbacks()
        {
            return await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackResponseDTO
                {
                    Id = f.Id,
                    Message = f.Message,
                    AdminReply = f.AdminReply,
                    CreatedAt = f.CreatedAt,
                    Status = f.Status
                })
                .ToListAsync();
        }

        public async Task<bool> ReplyFeedback(int feedbackId, string reply)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            feedback.AdminReply = reply;
            feedback.Status = "Replied";
            feedback.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFeedback(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
