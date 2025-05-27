using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class OrderItem
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }


        public Order Order { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        public MenuItem MenuItem { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
