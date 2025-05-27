using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class MenuItem
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public Guid RestaurantId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public Restaurant Restaurant { get; set; }
    }
}
