namespace Vortex.Models
{
    public class FeedbackResponseViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
        public string? AdminReply { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
    public class FeedbackDTO
    {
        public string Message { get; set; }
    }
}
