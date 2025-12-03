using KS_Sweets.Domain.Entities;

namespace KS_Sweets.Application.Contracts.DTOs
{
    public class HomePageDto
    {
        public List<Category> Categories { get; set; }
        public List<Feedback> Feedbacks { get; set; }
        public int CartCount { get; set; }
        public int WishlistCount { get; set; }
    }
}
