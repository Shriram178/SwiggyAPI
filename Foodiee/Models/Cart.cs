namespace Foodiee.Models
{
    public class Cart
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CartItem> CartItems { get; set; }
    }
}
