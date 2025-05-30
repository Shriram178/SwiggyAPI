using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string KeycloakId { get; set; }

        public Guid? RestaurantId { get; set; }

        public virtual Restaurant? Restaurant { get; set; }

        [Required]
        public string Username { get; set; } = null;

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }


    }
}
