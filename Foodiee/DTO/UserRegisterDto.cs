using System.ComponentModel.DataAnnotations;

namespace Foodiee.DTO;

// UserRegisterDto.cs
public class UserRegisterDto
{
    [Required]
    public string KeycloakId { get; set; }

    [Required]
    public string Email { get; set; }

}

// RestaurantRegisterDto.cs
public class RestaurantRegisterDto : UserRegisterDto
{
    [Required]
    public string RestaurantName { get; set; }

    [Required]
    public string RestaurantAddress { get; set; }
}
