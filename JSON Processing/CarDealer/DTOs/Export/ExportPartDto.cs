using Newtonsoft.Json;

namespace CarDealer.DTOs.Export
{
    [JsonObject]
    public class ExportPartDto
    {
        [JsonProperty("Name")] public string Name { get; set; } = null!;

        [JsonIgnore]
        public decimal Price { get; set; }

        [JsonProperty("Price")]
        public string FormattedPrice => string.Format($"{Price:F2}");
    }
}
