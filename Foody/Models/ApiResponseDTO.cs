using System.Text.Json.Serialization;

namespace StorySpoil.Models
{
    internal class ApiResponseDTO
    {
        //check if you need something here
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
