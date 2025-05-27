using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class Restaurant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public bool IsOpen { get; set; } = true;

        public List<MenuItem> MenuItems { get; set; }
    }
}
