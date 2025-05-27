namespace Foodiee.DTO
{
    public class RestaurantDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Description { get; set; }
    }
}
