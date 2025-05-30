using System.Text.Json.Serialization;

namespace Foodiee
{
    public partial class Program
    {
        public class RealmAccess
        {
            [JsonPropertyName("roles")]
            public List<string> Roles { get; set; } = new List<string>();
        }
    }
}