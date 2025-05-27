using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class CartItem
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CartId { get; set; }

        public Cart Cart { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        public MenuItem MenuItem { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
