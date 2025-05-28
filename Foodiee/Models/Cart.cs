using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class Cart
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public List<CartItem> CartItems { get; set; }
    }
}
