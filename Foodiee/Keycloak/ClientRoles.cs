using System.Text.Json.Serialization;

namespace Foodiee
{
    public partial class Program
    {
        public class ClientRoles
        {
            [JsonPropertyName("roles")] // This matches the JSON property name
            public List<string> Roles { get; set; } = new List<string>(); // Initialize to empty list
        }
    }
}