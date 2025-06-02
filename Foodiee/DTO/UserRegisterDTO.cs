using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Foodiee.DTO
{
    public class UserRegisterDto
    {
        [Required] public string Username { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }

        [Required] public string Address { get; set; }

        [DefaultValue(false)]
        public bool IsRestaurantOwner { get; set; }
    }
}
