namespace Foodiee.DTO
{
    public class CartDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDTO> Items { get; set; }
    }

}
