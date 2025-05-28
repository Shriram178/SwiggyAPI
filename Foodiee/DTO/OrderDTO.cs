using Foodiee.Models;

namespace Foodiee.DTO
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime PlacedAt { get; set; }
        public List<OrderItemDTO> Items { get; set; }
        public OrderStatus Status { get; set; }

    }

}
