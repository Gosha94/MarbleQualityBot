using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MarbleQualityBot.DTO.Requests;

public class OutlineObjectsRequest
{
    [Required]
    [JsonPropertyName("imageFile")]
    public IFormFile ImageFile { get; set; }

    [Required]
    [JsonPropertyName("predictionsJsonFile")]
    public IFormFile PredictionsJsonFile { get; set; }
}