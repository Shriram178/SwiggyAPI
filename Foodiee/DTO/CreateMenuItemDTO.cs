namespace Foodiee.DTO
{
    public class CreateMenuItemDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Guid RestaurantId { get; set; }

        public string Description { get; set; }
    }

}
