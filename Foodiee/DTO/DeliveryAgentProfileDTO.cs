namespace Foodiee.DTO
{
    public class DeliveryAgentProfileDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<OrderDTO> AssignedOrders { get; set; }
    }
}
