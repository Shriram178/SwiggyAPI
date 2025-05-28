namespace Foodiee.DTO
{
    public class MenuItemDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public string Description { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

}
