using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = null;

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Password { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public ICollection<Restaurant> Restaurants { get; set; }

    }
}
