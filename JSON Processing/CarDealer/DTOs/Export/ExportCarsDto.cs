using Newtonsoft.Json;

namespace CarDealer.DTOs.Export
{
    [JsonObject]
    public class ExportCarsDto
    {
        [JsonProperty("Make")] public string Make { get; set; } = null!;

        [JsonProperty("Model")] public string Model { get; set; } = null!;

        [JsonProperty("TravelledDistance")]
        public long TravelledDistance { get; set; }
    }
}
