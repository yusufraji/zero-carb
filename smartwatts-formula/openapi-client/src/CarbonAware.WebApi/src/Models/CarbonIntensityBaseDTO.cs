namespace CarbonAware.WebApi.Models;

using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

[Serializable]
public record CarbonIntensityBaseDTO
{
    /// <summary>the location name where workflow is run </summary>
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    [Required]
    public string? Location { get; set; } = string.Empty;

    /// <summary>the time at which the workflow we are measuring carbon intensity for started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [JsonPropertyName("startTime")]
    [Required]
    public DateTimeOffset? StartTime { get; set; }

    /// <summary> the time at which the workflow we are measuring carbon intensity for ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [JsonPropertyName("endTime")]
    [Required]
    public DateTimeOffset? EndTime { get; set; }

    

}