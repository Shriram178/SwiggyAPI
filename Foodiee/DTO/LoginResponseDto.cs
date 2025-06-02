namespace Foodiee.DTO
{
    public class LoginResponseDto
    {
        public string access_token { get; set; } = null!;
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string refresh_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
    }
}
