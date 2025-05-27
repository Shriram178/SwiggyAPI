namespace Foodiee.DTO
{
    public class CartItemDTO
    {
        public Guid Id { get; set; }
        public Guid MenuItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}
