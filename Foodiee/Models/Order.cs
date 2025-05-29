using System.ComponentModel.DataAnnotations;

namespace Foodiee.Models
{
    public class Order
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        public Guid? DeliveryAgentId { get; set; }

        public DeliveryAgentProfile? DeliveryAgent { get; set; }

        [Required]
        public Guid RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; }

        [Required]
        public List<OrderItem> OrderItems { get; set; }
    }
}
