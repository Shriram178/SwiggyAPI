using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class DeliveryAgentProfile
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string VehicleNumber { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}
