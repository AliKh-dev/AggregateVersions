
using System.Text.Json.Serialization;

namespace AggregateVersions.Presentation.Models
{
    public class AccessNode
    {
        [JsonPropertyName("id")]
        public string? ID { get; set; }

        [JsonPropertyName("text")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("children")]
        public List<AccessNode>? Children { get; set; }

        [JsonPropertyName("parents")]
        public List<AccessNode>? Parents { get; set; }

        [JsonIgnore()]
        public bool HaveChild => Children != null && Children.Count != 0;
    }
}
