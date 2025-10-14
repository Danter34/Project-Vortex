using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IFeedbackRepository
    {
        Task<FeedbackResponseDTO?> CreateFeedback(string userId, FeedbackDTO dto);
        Task<FeedbackResponseDTO?> GetFeedbackByUser(string userId);
        Task<List<FeedbackResponseDTO>> GetAllFeedbacks();
        Task<bool> ReplyFeedback(int feedbackId, string reply);
        Task<bool> DeleteFeedback(int feedbackId);
    }
}
