namespace Foodiee.DTO
{
    public class OrderItemDTO
    {
        public Guid MenuItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

    }

}
