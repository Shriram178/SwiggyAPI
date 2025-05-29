namespace Foodiee.DTO
{
    public class OrderDTO
    {

        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryAgentName { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public List<OrderItemDTO> OrderItems { get; set; } = new();

    }

}
