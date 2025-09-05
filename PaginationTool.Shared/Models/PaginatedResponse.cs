using System.Text.Json.Serialization;

namespace PaginationTool.Shared.Models
{
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new();

        [JsonPropertyName("nextLink")]
        public string? NextLink { get; set; }
    }
}
