namespace Vortex.Models
{
    public class ReviewViewModel
    {
        public string FullName { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
    public class addReview
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment
        {
            get; set;
        }
    }
}
